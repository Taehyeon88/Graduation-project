using System;
using System.Collections.Generic;
using UnityEngine;

public class MuscleTrophy : Perk
{
    [SerializeField] private int power_Amount = 1;
    public override void SubscribeCondition(Action<GameAction> reaction)
    {
        ActionSystem.SubscribeReaction<TurnGA>(reaction, ReactionTiming.POST);
    }

    public override void UnsubscribeCondition(Action<GameAction> reaction)
    {
        ActionSystem.UnsubscribeReaction<TurnGA>(reaction, ReactionTiming.POST);
    }
    public override bool SubConditionIsMat(GameAction action, HeroView owner)
    {
        var turnGA = action as TurnGA;
        if(turnGA.Type != TurnType.StartBattle) return false;
        return true;
    }
    public override void PerformReaction(GameAction action, HeroView owner)
    {
        AddStatusEffectGA addStatusEffectGA = new(StatusEffectType.POWER, power_Amount, new() { owner });
        ActionSystem.Instance.AddReaction(addStatusEffectGA);
    }
}
