using System;

//본인 사망 시 (reactionTiming은 PRE로 설정: KillGA가 토큰을 파괴하면 OnDisable에서 구독이 해제되므로 POST에서는 놓칠 수 있음)
public class SelfDeathCondition : TraitCondition
{
    public override void SubscribeCondition(Action<GameAction> reaction)
    {
        ActionSystem.SubscribeReaction<KillGA>(reaction, reactionTiming);
    }

    public override void UnsubscribeCondition(Action<GameAction> reaction)
    {
        ActionSystem.UnsubscribeReaction<KillGA>(reaction, reactionTiming);
    }

    public override bool SubConditionIsMat(GameAction action, CombatantView owner)
    {
        var killGA = action as KillGA;
        return killGA.Token == owner;
    }
}
