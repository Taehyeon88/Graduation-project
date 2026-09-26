using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealDamageGA : GameAction, IHaveCaster
{
    public float Amount { get; set; }
    public List<IDamageable> Targets { get; private set; }
    public IDamageable Target { get; private set; }
    public Token Caster { get; private set; }

    public DealDamageGA(float amount, List<IDamageable> targets, Token caster)
    {
        Amount = amount;
        Targets = new(targets);
        Caster = caster;
    }

    public DealDamageGA(float amount, IDamageable target, Token caster)
    {
        Amount = amount;
        Target = target;
        Caster = caster;
    }
}
