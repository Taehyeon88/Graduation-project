using System;

//손패 카드로 자신이 배치됐을 때 (전투 전 초기 배치·웨이브 생성은 PlayCardGA를 거치지 않아 제외)
public class SelfPlacedCondition : TraitCondition
{
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
        var playCardGA = action as PlayCardGA;
        return TokenSystem.Instance.API.GetTokenPosition(owner) == playCardGA.TargetPos;
    }
}
