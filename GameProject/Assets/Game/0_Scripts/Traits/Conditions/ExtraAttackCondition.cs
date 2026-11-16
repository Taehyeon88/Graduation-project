using System;

//자신이 추가 공격 차례를 얻었을 때 (공격 신호 등으로 예약된 ExtraAttackGA, 소유자 필터는 Trait.Reaction이 Caster로 처리)
//권장 reactionTiming: POST
public class ExtraAttackCondition : TraitCondition
{
    public override void SubscribeCondition(Action<GameAction> reaction)
    {
        ActionSystem.SubscribeReaction<ExtraAttackGA>(reaction, reactionTiming);
    }

    public override void UnsubscribeCondition(Action<GameAction> reaction)
    {
        ActionSystem.UnsubscribeReaction<ExtraAttackGA>(reaction, reactionTiming);
    }

    public override bool SubConditionIsMat(GameAction action, CombatantView owner)
    {
        return owner != null;   //파괴된 뷰 (해제 누락된 구독 방어)
    }
}
