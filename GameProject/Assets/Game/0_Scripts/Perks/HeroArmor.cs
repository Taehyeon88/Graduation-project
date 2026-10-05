using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HeroArmor : Perk
{
    [SerializeField] private int armor_Amount = 3;
    public override void SubscribeCondition(Action<GameAction> reaction)
    {
        ActionSystem.SubscribeReaction<AutoBattleTurnGA>(reaction, ReactionTiming.PRE);
    }

    public override void UnsubscribeCondition(Action<GameAction> reaction)
    {
        ActionSystem.UnsubscribeReaction<AutoBattleTurnGA>(reaction, ReactionTiming.PRE);
    }
    public override bool SubConditionIsMat(GameAction action, CombatantView owner)
    {
        return true;   //자동 전투 시작 전, 방어막 부여
    }
    public override void PerformReaction(GameAction action, CombatantView owner)
    {
        AddStatusEffectGA addStatusEffectGA = new(StatusEffectType.ARMOR, armor_Amount, new() { owner }, owner);
        ActionSystem.Instance.AddReaction(addStatusEffectGA);
    }
}
