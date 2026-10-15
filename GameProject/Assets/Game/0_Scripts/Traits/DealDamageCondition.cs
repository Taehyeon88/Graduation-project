using System;

public class DealDamageCondition : TraitCondition
{
    public override void SubscribeCondition(Action<GameAction> reaction)
    {
        ActionSystem.SubscribeReaction<DealDamageGA>(reaction, reactionTiming);
    }

    public override void UnsubscribeCondition(Action<GameAction> reaction)
    {
        ActionSystem.UnsubscribeReaction<DealDamageGA>(reaction, reactionTiming);
    }

    public override bool SubConditionIsMat(GameAction action)
    {
        return true;
    }
}
