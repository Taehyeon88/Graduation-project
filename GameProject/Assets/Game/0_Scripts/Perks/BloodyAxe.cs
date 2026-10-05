using System;
using System.Collections.Generic;
using UnityEngine;

public class BloodyAxe : Perk
{
    [SerializeField] private int added_Amount = 5;
    [SerializeField] private int heal_Amount = 5;
    public override void SubscribeCondition(Action<GameAction> reaction)
    {
        ActionSystem.SubscribeReaction<DealDamageGA>(reaction, ReactionTiming.POST);
    }

    public override void UnsubscribeCondition(Action<GameAction> reaction)
    {
        ActionSystem.UnsubscribeReaction<DealDamageGA>(reaction, ReactionTiming.POST);
    }
    public override bool SubConditionIsMat(GameAction action, CombatantView owner)
    {
        var dealDamageGA = action as DealDamageGA;
        if (dealDamageGA.Caster != owner) return false;
        if (dealDamageGA.Target == null && dealDamageGA.Targets == null) return false;   //대상 없는 공격(헛스윙)은 제외
        return true;
    }
    public override void PerformReaction(GameAction action, CombatantView owner)
    {
        var dealDamageGA = action as DealDamageGA;
        float r_value = UnityEngine.Random.Range(0.0f, 100.0f);

        if (dealDamageGA.Amount + added_Amount <= r_value)
        {
            Vector2Int pos = TokenSystem.Instance.API.GetTokenPosition(owner);
            HealGA healGA = new(heal_Amount, new() { pos }, owner);
            ActionSystem.Instance.AddReaction(healGA);
        }
    }
}
