using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AutoBattleSystem : Singleton<AutoBattleSystem>
{
    private const int DiceMax = 6;

    private List<CombatantView> battleQueue = new();   //행동 대기 큐 (속도 내림차순, 같은 속도는 주사위 값 내림차순)
    private readonly Dictionary<CombatantView, int> diceValues = new();   //같은 속도 유닛이 받은 주사위 값 (자동 전투 시작 때 한 번만 굴림)
    private readonly List<List<CombatantView>> pendingDiceGroups = new();   //아직 연출하지 않은 같은 속도 그룹 (그룹이 처음 행동하는 순간에 한 번 연출)

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

    public IReadOnlyList<CombatantView> BattleQueue => battleQueue;   //아직 행동하지 않은 유닛 (행동 순서, 튜토리얼 행동 순서 띠 표시용)

    //그리드 위 이동 가능한 CombatantView를 행동 순서대로 캐싱 (Speed 0은 가장 늦게 행동, 이동 불가(-1)는 제외)
    public void CacheCombatants()
    {
        List<CombatantView> combatants = TokenSystem.Instance.API.GetAllTokens()
            .OfType<CombatantView>()
            .Where(c => !c.IsImmobile)
            .ToList();

        //같은 속도 그룹마다 주사위를 한 번만 굴려 순서를 확정
        diceValues.Clear();
        pendingDiceGroups.Clear();
        foreach (var group in combatants.GroupBy(c => c.Speed).Where(g => g.Count() > 1))
            RollDice(group.ToList());

        battleQueue = combatants
            .OrderByDescending(c => c.Speed)
            .ThenByDescending(c => diceValues.TryGetValue(c, out int value) ? value : 0)
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

        CombatantView unit = battleQueue[0];

        //같은 속도 그룹이 처음 행동하는 순간에 주사위 연출 (그룹당 한 번)
        List<CombatantView> diceGroup = pendingDiceGroups.Find(g => g.Contains(unit));
        if (diceGroup != null)
        {
            pendingDiceGroups.Remove(diceGroup);
            yield return PlayDiceRoll(diceGroup);
        }

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

    //같은 속도 그룹에 서로 다른 1~6 주사위 값을 배정 (6명을 넘으면 값이 겹치므로 무작위 순서만 정하고 연출하지 않음)
    private void RollDice(List<CombatantView> group)
    {
        List<int> values = group.Count <= DiceMax
            ? Enumerable.Range(1, DiceMax).OrderBy(_ => Random.value).Take(group.Count).ToList()
            : Enumerable.Range(1, group.Count).OrderBy(_ => Random.value).ToList();

        for (int i = 0; i < group.Count; i++)
            diceValues[group[i]] = values[i];

        //튜토리얼: 지정 라운드에 지정 영웅이 그룹에 있으면 그 영웅이 가장 큰 값을 받도록 값을 맞바꿈
        TutorialBattleData tutorial = GameSystem.Instance.TutorialBattle;
        if (tutorial != null && tutorial.SameSpeedWinner != null && TurnSystem.Instance.CurrentTurn_Number == tutorial.SameSpeedRound)
        {
            CombatantView winner = group.Find(c => c.TokenData == tutorial.SameSpeedWinner);
            if (winner != null)
            {
                CombatantView highest = group.OrderByDescending(c => diceValues[c]).First();
                (diceValues[winner], diceValues[highest]) = (diceValues[highest], diceValues[winner]);
            }
        }

        if (group.Count <= DiceMax)
            pendingDiceGroups.Add(group);
    }

    private IEnumerator PlayDiceRoll(List<CombatantView> group)
    {
        List<CombatantView> alive = group.Where(c => c != null && battleQueue.Contains(c)).ToList();
        if (alive.Count < 2) yield break;   //앞선 유닛의 공격으로 그룹이 흩어졌으면 연출 생략

        TutorialSystem.NotifySameSpeedTie(alive);   //튜토리얼: 같은 속도 설명 시점 (설명이 끝날 때까지 연출 대기)
        while (ActionSystem.Instance.IsPausing)
            yield return null;

        if (DiceRollUI.Instance != null)
            yield return DiceRollUI.Instance.Play(alive.Select(c => (c, diceValues[c])).ToList());
    }
}
