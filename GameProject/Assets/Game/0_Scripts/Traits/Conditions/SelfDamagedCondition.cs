using System;

//소유자가 피해를 실제로 입었을 때 — 파괴되는 타격 포함 (reactionTiming은 POST 권장: DamagedTargets가 채워진 뒤, KillGA 수행 전에 판정됨. 상대가 Caster이므로 Caster 필터를 끈다)
public class SelfDamagedCondition : TraitCondition
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
        return dealDamageGA.DamagedTargets.Contains(owner);   //회피로 건너뛴 피격은 DamagedTargets에 없음
    }
}
