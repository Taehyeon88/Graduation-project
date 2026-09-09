using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealDamageGA : GameAction, IHaveCaster
{
    public float Amount { get; set; }
    public List<CombatantView> Targets { get; private set; }
    public CombatantView Target { get; private set; }
    public CombatantView Caster { get; private set; }

    public DealDamageGA(float amount, List<CombatantView> targets, CombatantView caster)
    {
        Amount = amount;
        Targets = new(targets);
        Caster = caster;
    }

    public DealDamageGA(float amount, CombatantView target, CombatantView caster)
    {
        Amount = amount;
        Target = target;
        Caster = caster;
    }
}
