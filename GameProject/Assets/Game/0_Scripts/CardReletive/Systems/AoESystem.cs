using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IsoTools;
using UnityEngine;

public class AoESystem : Singleton<AoESystem>
{
    [SerializeField] private AoEData[] aoEDatas;
    private Dictionary<AoEType, AoEData> aoEDataByType = new();          //타입 별, 영역 데이터
    private Dictionary<Vector2Int, AoE> fieldAoEsByPosition = new();     //필드 계열의 영역 장판
    private Dictionary<Vector2Int, AoE> objectAoEsByPosition = new();    //오브젝트 계열의 영역 장판

    private void OnEnable()
    {
        //effectData들 타입별로 캐싱
        foreach (var data in aoEDatas)
        {
            if (!aoEDataByType.TryAdd(data.AoEType, data))
                Debug.LogError($"{data.name}데이터의 effectType이 {aoEDataByType[data.AoEType].name}과 {data.AoEType}으로 충돌함");
        }

        ActionSystem.AttachPerformer<AddAoEGA>(AddAoEGAPerformer);
        ActionSystem.SubscribeReaction<EnemysTurnGA>(EnemysTurnGAPostReaction, ReactionTiming.POST);
        ActionSystem.SubscribeReaction<EnemysTurnGA>(EnemysTurnGAPreReaction, ReactionTiming.PRE);
        ActionSystem.SubscribeReaction<EnemyTurnGA>(EnemyTurnGAPreReaction, ReactionTiming.PRE);
        ActionSystem.SubscribeReaction<MoveGA>(MoveGAPostReaction, ReactionTiming.POST);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<AddAoEGA>();
        ActionSystem.UnsubscribeReaction<EnemysTurnGA>(EnemysTurnGAPostReaction, ReactionTiming.POST);
        ActionSystem.UnsubscribeReaction<EnemysTurnGA>(EnemysTurnGAPreReaction, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<EnemyTurnGA>(EnemyTurnGAPreReaction, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<MoveGA>(MoveGAPostReaction, ReactionTiming.POST);
    }

    //Performer
    private IEnumerator AddAoEGAPerformer(AddAoEGA addAoEGA)
    {
        if (aoEDataByType.ContainsKey(addAoEGA.AoEType))
        {
            var data = aoEDataByType[addAoEGA.AoEType];
            var aoE = new AoE(data, addAoEGA.Caster);

            foreach (var targetPos in addAoEGA.TargetPoses)
            {
                //예외 : 같은 계열의 장판이 이미 위치에 있을 경우, 덮어씌움
                if (data.AoEFieldType == AoEFieldType.Field)
                {
                    if (fieldAoEsByPosition.ContainsKey(targetPos))
                        fieldAoEsByPosition[targetPos] = aoE;
                    else 
                        fieldAoEsByPosition.Add(targetPos, aoE);
                }
                else if (data.AoEFieldType == AoEFieldType.Object)
                {
                    if (objectAoEsByPosition.ContainsKey(targetPos))
                        objectAoEsByPosition[targetPos] = aoE;
                    else
                        objectAoEsByPosition.Add(targetPos, aoE);
                }

                //해당 타일 생성 및 배치
                IsoObject aoEObject = AoECreator.Instance.CreateAoE(aoE.AoEModel);
                TokenSystem.Instance.SetAoE(aoEObject, targetPos, aoE.AoEFieldType);

                //지정한 타일 내, 대상이 존재할 경우
                CombatantView target = TokenSystem.Instance.GetTokenByPosition(targetPos) as CombatantView;
                if(target != null)
                    CheckAndPlayAoE(target);
            }
        }
        else Debug.LogError($"{addAoEGA.AoEType}타입의 AoEData를 찾을 수 없습니다.");
        yield return null;
    }

    //Reactions

    //플레이어 턴 시작시, 체크
    private void EnemysTurnGAPostReaction(EnemysTurnGA enemysTurnGA)
    {
        var hero = HeroSystem.Instance.HeroView;
        if (hero != null)
            CheckAndPlayAoE(hero);

        //플레이어가 사용한 필드AoE 지속 턴 감소
        foreach (var pos in fieldAoEsByPosition.Keys.ToList())
        {
            var aoE = fieldAoEsByPosition[pos];
            if (aoE.Caster is HeroView)
            {
                if (aoE.ReduceRemainDurationTurn() <= 0)
                {
                    //AoE 이미지 및 데이터 삭제 처리
                    TokenSystem.Instance.ResetAoE(pos, aoE.AoEFieldType);
                    fieldAoEsByPosition.Remove(pos);
                }
            }
        }

        //플레이어가 사용한 오브젝트AoE 지속 턴 감소
    }

    //몬스터들 턴 시작시, 체크
    private void EnemysTurnGAPreReaction(EnemysTurnGA enemysTurnGA)
    {
        //몬스터가 사용한 필드AoE 지속 턴 감소
        foreach (var pos in fieldAoEsByPosition.Keys.ToList())
        {
            var aoE = fieldAoEsByPosition[pos];
            if (aoE.Caster is EnemyView)
            {
                if (aoE.ReduceRemainDurationTurn() <= 0)
                {
                    //AoE 이미지 및 데이터 삭제 처리
                    TokenSystem.Instance.ResetAoE(pos, aoE.AoEFieldType);
                    fieldAoEsByPosition.Remove(pos);
                }
            }
        }

        //몬스터가 사용한 오브젝트AoE 지속 턴 감소
    }

    //몬스터 턴 시작시, 체크
    private void EnemyTurnGAPreReaction(EnemyTurnGA enemyTurnGA)
    {
        var enemy = enemyTurnGA.EnemyView;
        if (enemy != null)
            CheckAndPlayAoE(enemy);
    }

    //영웅, 적 모두 이동 이후 체크
    private void MoveGAPostReaction(MoveGA moveGA)
    {
        var mover = moveGA.mover;
        if (mover != null)
            CheckAndPlayAoE(mover);
    }

    //Privates
    private void CheckAndPlayAoE(CombatantView target)
    {
        Vector2Int pos = TokenSystem.Instance.GetTokenPosition(target);

        //필드 계열 장판 처리
        if (fieldAoEsByPosition.ContainsKey(pos))
            ApplyAreaOfEffects(target, fieldAoEsByPosition[pos], false);

        //오브젝트 계열 장판 처리
        if (objectAoEsByPosition.ContainsKey(pos))
        {
            if (ApplyAreaOfEffects(target, objectAoEsByPosition[pos], false))
            {
                //영역 효과가 오브젝트 계열일 경우, 삭제처리
                if (objectAoEsByPosition[pos].AoEFieldType == AoEFieldType.Object)
                {
                    objectAoEsByPosition.Remove(pos);
                }
            }
        }
    }
    private bool ApplyAreaOfEffects(CombatantView target, AoE aoE, bool isEntry)
    {
        if (aoE == null) return false;
        if (aoE.Caster == null) return false;

        //틱 데미지 적용
        int damage = 0;
        if (isEntry) damage = aoE.EntryDamage;
        else damage = aoE.TurnDamage;

        if (damage > 0)
        {
            DealDamageGA dealDamageGA = new(damage, new() { target }, aoE.Caster);
            ActionSystem.Instance.AddReaction(dealDamageGA);
        }

        //상태 효과 적용
        foreach (var effect in aoE.statusEffects)
        {
            if (effect is AddStatusEffectEffect)
            {
                PerformEffectGA performEffectGA = new(effect, new(new List<CombatantView>() { target }, aoE.Caster));
                ActionSystem.Instance.AddReaction(performEffectGA);
            }
        }

        return true;
    }
}
