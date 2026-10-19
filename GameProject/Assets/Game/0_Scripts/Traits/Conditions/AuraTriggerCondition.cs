using System;

//같은 진영 유닛(자신 포함)이 손패 카드로 배치됐을 때 또는 자신이 사망할 때 (지속 오라의 부여·회수 시점)
//reactionTiming은 PlayCardGA에만 적용(권장 POST: AddToken 이후라 위치 조회 가능), KillGA는 PRE 고정 (POST는 OnDisable 구독 해제로 놓칠 수 있음)
public class AuraTriggerCondition : TraitCondition
{
    private const ReactionTiming KillTiming = ReactionTiming.PRE;

    public override void SubscribeCondition(Action<GameAction> reaction)
    {
        ActionSystem.SubscribeReaction<PlayCardGA>(reaction, reactionTiming);
        ActionSystem.SubscribeReaction<KillGA>(reaction, KillTiming);
    }

    public override void UnsubscribeCondition(Action<GameAction> reaction)
    {
        ActionSystem.UnsubscribeReaction<PlayCardGA>(reaction, reactionTiming);
        ActionSystem.UnsubscribeReaction<KillGA>(reaction, KillTiming);
    }

    public override bool SubConditionIsMat(GameAction action, CombatantView owner)
    {
        if (owner == null) return false;   //파괴된 뷰 (해제 누락된 구독 방어)

        if (action is KillGA killGA) return killGA.Token == owner;

        var playCardGA = action as PlayCardGA;
        Token placed = TokenSystem.Instance.API.GetTokenByPosition(playCardGA.TargetPos);
        return owner is HeroView ? placed is HeroView : placed is EnemyView;
    }
}
