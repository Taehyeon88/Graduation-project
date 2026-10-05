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
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<AutoBattleGA>();
    }

    //그리드 위 모든 CombatantView를 속도순으로 캐싱 (Speed 0 유닛은 행동하지 않음)
    public void CacheCombatants()
    {
        battleQueue = TokenSystem.Instance.API.GetAllTokens()
            .OfType<CombatantView>()
            .Where(c => c.Speed > 0)
            .OrderByDescending(c => c.Speed)
            .ToList();
    }

    //Performer
    private IEnumerator AutoBattleGAPerformer(AutoBattleGA autoBattleGA)
    {
        battleQueue.RemoveAll(c => c == null || !TokenSystem.Instance.API.IsTokenExist(c));   //제거된 유닛 정리

        //한쪽이 전멸하면 자동 전투 종료
        if (TokenSystem.Instance.HeroViews.Count <= 0 || TokenSystem.Instance.EnemyViews.Count <= 0)
            battleQueue.Clear();
        if (battleQueue.Count <= 0) yield break;

        CombatantView unit = PickNext();
        battleQueue.Remove(unit);

        yield return unit.Battle();   //행동(MoveGA, DealDamageGA 등) 예약

        //남은 유닛이 있으면, 예약한 행동 뒤에 다음 자동 전투 실행
        if (battleQueue.Count > 0)
            ActionSystem.Instance.AddReaction(new AutoBattleGA());
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
        Debug.Log($"동속 {group.Count}명 주사위: {string.Join(", ", dice)}");
        return group[best];
    }
}
