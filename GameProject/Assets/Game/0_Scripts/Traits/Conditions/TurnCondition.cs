using System;
using UnityEngine;

public class TurnCondition : TraitCondition
{
    [SerializeField] private TurnType turnType;

    public override void SubscribeCondition(Action<GameAction> reaction)
    {
        ActionSystem.SubscribeReaction<TurnGA>(reaction, reactionTiming);
    }

    public override void UnsubscribeCondition(Action<GameAction> reaction)
    {
        ActionSystem.UnsubscribeReaction<TurnGA>(reaction, reactionTiming);
    }

    public override bool SubConditionIsMat(GameAction action, CombatantView owner)
    {
        var turnGA = action as TurnGA;
        return turnGA.Type == turnType;
    }
}
