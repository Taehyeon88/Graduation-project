using UnityEngine;

public class HealOnDamageEffect : TraitEffect
{
    [SerializeField] private int added_Amount = 5;
    [SerializeField] private int heal_Amount = 5;

    public override void PerformReaction(GameAction action, CombatantView owner)
    {
        var dealDamageGA = action as DealDamageGA;
        float r_value = UnityEngine.Random.Range(0.0f, 100.0f);

        if (dealDamageGA.Amount + added_Amount <= r_value)
        {
            Vector2Int pos = TokenSystem.Instance.API.GetTokenPosition(owner);
            HealGA healGA = new(heal_Amount, new() { pos }, owner);
            ActionSystem.Instance.AddReaction(healGA);
        }
    }
}
