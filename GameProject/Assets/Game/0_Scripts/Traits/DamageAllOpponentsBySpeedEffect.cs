using System.Collections.Generic;
using System.Linq;

public class DamageAllOpponentsBySpeedEffect : TraitEffect
{
    public override void PerformReaction(GameAction action, CombatantView owner)
    {
        int amount = owner.Speed;

        List<IDamageable> targets = owner is HeroView
            ? TokenSystem.Instance.EnemyViews.Cast<IDamageable>().ToList()
            : TokenSystem.Instance.HeroViews.Cast<IDamageable>().ToList();

        if (targets.Count > 0)
            ActionSystem.Instance.AddReaction(new DealDamageGA(amount, targets, owner));
        ActionSystem.Instance.AddReaction(new AttackCoreGA(owner, amount));
    }
}
