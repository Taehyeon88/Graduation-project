using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AutoBattleSystem : Singleton<AutoBattleSystem>
{
    private const int DiceMax = 6;

    private List<CombatantView> battleQueue = new();   //속도 내림차순 행동 대기 큐

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<AutoBattleGA>(AutoBattleGAPerformer);
        ActionSystem.AttachPerformer<ExtraAttackGA>(ExtraAttackGAPerformer);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<AutoBattleGA>();
        ActionSystem.DetachPerformer<ExtraAttackGA>();
    }

    //그리드 위 이동 가능한 CombatantView를 속도순으로 캐싱 (Speed 0은 가장 늦게 행동, 이동 불가(-1)는 제외)
    public void CacheCombatants()
    {
        battleQueue = TokenSystem.Instance.API.GetAllTokens()
            .OfType<CombatantView>()
            .Where(c => !c.IsImmobile)
            .OrderByDescending(c => c.Speed)
            .ToList();
    }

    //Performer
    private IEnumerator AutoBattleGAPerformer(AutoBattleGA autoBattleGA)
    {
        battleQueue.RemoveAll(c => c == null || !TokenSystem.Instance.API.IsTokenExist(c));   //제거된 유닛 정리

        //코어가 파괴되어 승패가 정해지면 자동 전투 종료
        if (GameSystem.Instance.IsGameClear || GameSystem.Instance.IsGameOver)
            battleQueue.Clear();
        if (battleQueue.Count <= 0) yield break;

        CombatantView unit = PickNext();
        battleQueue.Remove(unit);

        //기절 : 이번 턴 행동 불가 (스택은 턴 종료 시 감소)
        if (!unit.CheckStatusEffectExist(StatusEffectType.STUN))
            yield return unit.Battle();   //행동(MoveGA, DealDamageGA 등) 예약

        //남은 유닛이 있으면, 예약한 행동 뒤에 다음 자동 전투 실행
        if (battleQueue.Count > 0)
            ActionSystem.Instance.AddReaction(new AutoBattleGA());
    }

    //추가 공격 차례 (이동 없이 현재 위치에서 공격 예약)
    private IEnumerator ExtraAttackGAPerformer(ExtraAttackGA extraAttackGA)
    {
        CombatantView caster = extraAttackGA.Caster;
        if (caster == null || !TokenSystem.Instance.API.IsTokenExist(caster)) yield break;
        if (GameSystem.Instance.IsGameClear || GameSystem.Instance.IsGameOver) yield break;
        if (caster.CheckStatusEffectExist(StatusEffectType.STUN)) yield break;   //기절 : 추가 공격도 불가

        caster.ReserveAttack(TokenSystem.Instance.API.GetTokenPosition(caster));
    }

    //가장 빠른 유닛 선택 (동속이면 서로 다른 1~6 주사위로 순서 결정)
    private CombatantView PickNext()
    {
        int topSpeed = battleQueue[0].Speed;
        List<CombatantView> group = battleQueue.Where(c => c.Speed == topSpeed).ToList();
        if (group.Count == 1) return group[0];

        if (group.Count > DiceMax)   //주사위 값이 겹치지 않게 할 수 없으면 무작위 선택
            return group[Random.Range(0, group.Count)];

        List<int> dice = Enumerable.Range(1, DiceMax).OrderBy(_ => Random.value).Take(group.Count).ToList();
        int best = dice.IndexOf(dice.Max());
        return group[best];
    }
}
