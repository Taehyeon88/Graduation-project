using UnityEngine;
using UnityEngine.Serialization;

public class HealOnDamageEffect : TraitEffect
{
    [FormerlySerializedAs("added_Amount")]
    [SerializeField] private int addedAmount = 5;
    [FormerlySerializedAs("heal_Amount")]
    [SerializeField] private int healAmount = 5;

    public override void PerformReaction(GameAction action, CombatantView owner)
    {
        var dealDamageGA = action as DealDamageGA;
        float r_value = UnityEngine.Random.Range(0.0f, 100.0f);

        if (dealDamageGA.Amount + addedAmount <= r_value)
        {
            Vector2Int pos = TokenSystem.Instance.API.GetTokenPosition(owner);
            HealGA healGA = new(healAmount, new() { pos }, owner);
            ActionSystem.Instance.AddReaction(healGA);
        }
    }
}
