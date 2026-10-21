//코어를 공격한 상대에게 소유자의 공격력만큼 반격
public class RetaliateEffect : TraitEffect
{
    public override void PerformReaction(GameAction action, CombatantView owner)
    {
        var attackCoreGA = action as AttackCoreGA;
        if (attackCoreGA.Caster is not IDamageable attacker) return;

        ActionSystem.Instance.AddReaction(new DealDamageGA(owner.BaseDamage, attacker, owner, true));
    }
}
