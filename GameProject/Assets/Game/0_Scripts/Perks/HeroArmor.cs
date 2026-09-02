using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HeroArmor : Perk
{
    [SerializeField] private int armor_Amount = 3;
    public override void SubscribeCondition(Action<GameAction> reaction)
    {
        ActionSystem.SubscribeReaction<TurnGA>(reaction, ReactionTiming.PRE);
    }

    public override void UnsubscribeCondition(Action<GameAction> reaction)
    {
        ActionSystem.UnsubscribeReaction<TurnGA>(reaction, ReactionTiming.PRE);
    }
    public override bool SubConditionIsMat(GameAction action, HeroView owner)
    {
        var turnGA = action as TurnGA;
        if (turnGA.Type != TurnType.Enemy) return false;
        return true;
    }
    public override void PerformReaction(GameAction action, HeroView owner)
    {
        AddStatusEffectGA addStatusEffectGA = new(StatusEffectType.ARMOR, armor_Amount, new() { owner });
        ActionSystem.Instance.AddReaction(addStatusEffectGA);
    }
}
