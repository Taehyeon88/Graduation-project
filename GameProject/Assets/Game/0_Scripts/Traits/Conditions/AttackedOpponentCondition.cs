using System;

//상대 진영 유닛 또는 상대 코어를 공격한 뒤 (소유자 필터는 Trait.Reaction이 Caster로 처리)
public class AttackedOpponentCondition : TraitCondition
{
    public override void SubscribeCondition(Action<GameAction> reaction)
    {
        ActionSystem.SubscribeReaction<DealDamageGA>(reaction, reactionTiming);
        ActionSystem.SubscribeReaction<AttackCoreGA>(reaction, reactionTiming);
    }

    public override void UnsubscribeCondition(Action<GameAction> reaction)
    {
        ActionSystem.UnsubscribeReaction<DealDamageGA>(reaction, reactionTiming);
        ActionSystem.UnsubscribeReaction<AttackCoreGA>(reaction, reactionTiming);
    }

    public override bool SubConditionIsMat(GameAction action, CombatantView owner)
    {
        if (action is AttackCoreGA) return true;

        var dealDamageGA = action as DealDamageGA;
        if (dealDamageGA.Target != null && IsOpponent(dealDamageGA.Target, owner)) return true;
        if (dealDamageGA.Targets != null)
        {
            foreach (var target in dealDamageGA.Targets)
                if (target != null && IsOpponent(target, owner)) return true;
        }
        return false;
    }

    private bool IsOpponent(IDamageable target, CombatantView owner)
    {
        return owner is HeroView ? target is EnemyView : target is HeroView;
    }
}
