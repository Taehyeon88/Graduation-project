using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealDamageGA : GameAction, IHaveCaster
{
    public float Amount { get; set; }
    public List<IDamage> Targets { get; private set; }
    public IDamage Target { get; private set; }
    public Token Caster { get; private set; }

    public DealDamageGA(float amount, List<IDamage> targets, Token caster)
    {
        Amount = amount;
        Targets = new(targets);
        Caster = caster;
    }

    public DealDamageGA(float amount, IDamage target, Token caster)
    {
        Amount = amount;
        Target = target;
        Caster = caster;
    }
}
