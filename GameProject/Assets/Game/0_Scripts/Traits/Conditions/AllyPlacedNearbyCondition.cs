using System;
using UnityEngine;

//자신의 주변에 손패의 아군 용병이 배치될 때 (자기 자신의 배치는 제외, 영웅만 해당)
public class AllyPlacedNearbyCondition : TraitCondition
{
    [SerializeField] private int range = 1;   //주변 범위 (8방향 기준 칸 수)

    public override void SubscribeCondition(Action<GameAction> reaction)
    {
        ActionSystem.SubscribeReaction<PlayCardGA>(reaction, reactionTiming);
    }

    public override void UnsubscribeCondition(Action<GameAction> reaction)
    {
        ActionSystem.UnsubscribeReaction<PlayCardGA>(reaction, reactionTiming);
    }

    public override bool SubConditionIsMat(GameAction action, CombatantView owner)
    {
        if (owner is not HeroView) return false;

        var playCardGA = action as PlayCardGA;
        Vector2Int delta = playCardGA.TargetPos - TokenSystem.Instance.API.GetTokenPosition(owner);
        int distance = Mathf.Max(Mathf.Abs(delta.x), Mathf.Abs(delta.y));
        return distance >= 1 && distance <= range;   //0이면 자기 자신의 배치
    }
}
