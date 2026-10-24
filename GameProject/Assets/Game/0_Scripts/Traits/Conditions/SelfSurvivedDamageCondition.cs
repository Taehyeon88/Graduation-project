using System;

//소유자가 피해를 실제로 입고 살아남았을 때 (reactionTiming은 POST 권장: 피해 적용 뒤라야 생존 여부를 알 수 있음. 상대가 Caster이므로 Caster 필터를 끈다)
public class SelfSurvivedDamageCondition : TraitCondition
{
    public override bool FiltersByCaster => false;

    public override void SubscribeCondition(Action<GameAction> reaction)
    {
        ActionSystem.SubscribeReaction<DealDamageGA>(reaction, reactionTiming);
    }

    public override void UnsubscribeCondition(Action<GameAction> reaction)
    {
        ActionSystem.UnsubscribeReaction<DealDamageGA>(reaction, reactionTiming);
    }

    public override bool SubConditionIsMat(GameAction action, CombatantView owner)
    {
        var dealDamageGA = action as DealDamageGA;
        return dealDamageGA.DamagedTargets.Contains(owner) && owner.CurrentHealth > 0;   //회피로 건너뛴 피격은 DamagedTargets에 없음
    }
}
