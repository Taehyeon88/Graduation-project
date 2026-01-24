using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealDamageEffect : Effect
{
    [SerializeField] private int amount;
    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        DealDamageGA dealDamageGA = new(amount, targets, caster);
        return dealDamageGA;
    }
}
