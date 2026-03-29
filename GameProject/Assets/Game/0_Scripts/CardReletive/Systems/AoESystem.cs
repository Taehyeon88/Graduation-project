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
            var aoE = new AoE(data, addAoEGA.Caster, addAoEGA.AoETargetMode);

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
                if (aoEObject != null)
                    TokenSystem.Instance.SetAoE(aoEObject, targetPos, aoE.AoEFieldType);
                else
                    Debug.LogError($"{aoE.AoEType}의 인스턴스가 생성되지 않았습니다.");

                //지정한 타일 내, 대상이 존재할 경우
                CombatantView target = TokenSystem.Instance.GetTokenByPosition(targetPos) as CombatantView;
                if(target != null)
                    CheckAndPlayAoE(target, true);
            }
        }
        else Debug.LogError($"{addAoEGA.AoEType}타입의 AoEData를 찾을 수 없습니다.");
        yield return null;
    }

    //Reactions

    //플레이어 턴 시작시, 체크
    private void EnemysTurnGAPostReaction(EnemysTurnGA enemysTurnGA)
    {
        //플레이어가 사용한 필드AoE 지속 턴 감소
        foreach (var pos in fieldAoEsByPosition.Keys.ToList())
        {
            var aoE = fieldAoEsByPosition[pos];
            if (aoE.CasterType == TokenType.Hero)
            {
                if (aoE.ReduceRemainDuration() <= 0)
                {
                    //AoE 이미지 및 데이터 삭제 처리
                    TokenSystem.Instance.ResetAoE(pos, aoE.AoEFieldType);
                    fieldAoEsByPosition.Remove(pos);
                }
            }
        }

        //플레이어가 사용한 오브젝트AoE 지속 턴 감소
        foreach (var pos in objectAoEsByPosition.Keys.ToList())
        {
            var aoE = objectAoEsByPosition[pos];
            if (aoE.CasterType == TokenType.Hero && !aoE.UseCountBased)
            {
                if (aoE.ReduceRemainDuration() <= 0)
                {
                    //AoE 이미지 및 데이터 삭제 처리
                    TokenSystem.Instance.ResetAoE(pos, aoE.AoEFieldType);
                    objectAoEsByPosition.Remove(pos);
                }
            }
        }

        var hero = HeroSystem.Instance.HeroView;
        if (hero != null)
            CheckAndPlayAoE(hero, false);
    }

    //몬스터들 턴 시작시, 체크
    private void EnemysTurnGAPreReaction(EnemysTurnGA enemysTurnGA)
    {
        //몬스터가 사용한 필드AoE 지속 턴 감소
        foreach (var pos in fieldAoEsByPosition.Keys.ToList())
        {
            var aoE = fieldAoEsByPosition[pos];
            if (aoE.CasterType == TokenType.Enemy)
            {
                if (aoE.ReduceRemainDuration() <= 0)
                {
                    //AoE 이미지 및 데이터 삭제 처리
                    TokenSystem.Instance.ResetAoE(pos, aoE.AoEFieldType);
                    fieldAoEsByPosition.Remove(pos);
                }
            }
        }

        //몬스터가 사용한 오브젝트AoE 지속 턴 감소
        foreach (var pos in objectAoEsByPosition.Keys.ToList())
        {
            var aoE = objectAoEsByPosition[pos];
            if (aoE.CasterType == TokenType.Enemy && !aoE.UseCountBased)
            {
                if (aoE.ReduceRemainDuration() <= 0)
                {
                    //AoE 이미지 및 데이터 삭제 처리
                    TokenSystem.Instance.ResetAoE(pos, aoE.AoEFieldType);
                    objectAoEsByPosition.Remove(pos);
                }
            }
        }
    }

    //몬스터 턴 시작시, 체크
    private void EnemyTurnGAPreReaction(EnemyTurnGA enemyTurnGA)
    {
        var enemy = enemyTurnGA.EnemyView;
        if (enemy != null)
            CheckAndPlayAoE(enemy, false);
    }

    //영웅, 적 모두 이동 이후 체크
    private void MoveGAPostReaction(MoveGA moveGA)
    {
        var mover = moveGA.mover;
        if (mover != null)
            CheckAndPlayAoE(mover, true);
    }

    //Privates
    private void CheckAndPlayAoE(CombatantView target, bool isEntry)
    {
        Vector2Int pos = TokenSystem.Instance.GetTokenPosition(target);

        //필드 계열 장판 처리
        if (fieldAoEsByPosition.ContainsKey(pos))
        {
            var aoE = fieldAoEsByPosition[pos];
            if (aoE != null)
            {
                ApplyAreaOfEffects(target, aoE);
                ApplyAoEDamage(target, aoE, isEntry);
            }
        }

        //오브젝트 계열 장판 처리
        if (objectAoEsByPosition.ContainsKey(pos))
        {
            var aoE = objectAoEsByPosition[pos];
            if (aoE != null)
            {
                if (ApplyAreaOfEffects(target, aoE))
                {
                    //영역 효과가 오브젝트 계열 + 횟수기반일 경우, 횟수 감소 및 횟수가 없을 시, 삭제처리
                    if (aoE.AoEFieldType == AoEFieldType.Object && aoE.UseCountBased)
                    {
                        if (aoE.ReduceRemainDuration() <= 0)
                        {
                            TokenSystem.Instance.ResetAoE(pos, aoE.AoEFieldType);
                            objectAoEsByPosition.Remove(pos);
                        }
                    }
                }
                ApplyAoEDamage(target, aoE, isEntry);
            }
        }
    }
    private bool ApplyAreaOfEffects(CombatantView target, AoE aoE)
    {
        var targetType = aoE.GetTokenType(target);

        if (aoE.CheckCanUse(target))
        {
            //상태 효과 적용
            foreach (var effect in aoE.effects)
            {
                PerformEffectGA performEffectGA = new(effect, new(new List<CombatantView>() { target }, aoE.Caster));
                ActionSystem.Instance.AddReaction(performEffectGA);
            }
            return true;
        }
        return false;
    }

    private void ApplyAoEDamage(CombatantView target, AoE aoE, bool isEntry)
    {
        var targetType = aoE.GetTokenType(target);

        //틱 데미지 적용
        int damage = 0;
        if (isEntry) damage = aoE.EntryDamage;
        else damage = aoE.TurnDamage;

        if (damage > 0)
        {
            //몬스터는 영웅에게 | 영웅은 몬스터에게만 데미지 피격 가능
            if (targetType != aoE.CasterType)
            {
                DealDamageGA dealDamageGA = new(damage, new() { target }, aoE.Caster);
                ActionSystem.Instance.AddReaction(dealDamageGA);
            }
        }
    }
}
