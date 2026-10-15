using UnityEngine;

public class AddStatusTraitEffect : TraitEffect
{
    [SerializeField] private StatusEffectType statusEffectType;
    [SerializeField] private int amount = 1;

    public override void PerformReaction(GameAction action, CombatantView owner)
    {
        AddStatusEffectGA addStatusEffectGA = new(statusEffectType, amount, new() { owner }, owner);
        ActionSystem.Instance.AddReaction(addStatusEffectGA);
    }
}
