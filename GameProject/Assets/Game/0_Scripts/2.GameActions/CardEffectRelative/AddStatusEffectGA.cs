using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddStatusEffectGA : GameAction, IHaveCaster
{
    public StatusEffectType StatusEffectType { get; private set; }
    public int StackCount { get; set; }
    public List<CombatantView> Targets { get; private set; }
    public HeroView Caster { get; private set; }
    Token IHaveCaster.Caster => Caster;
    public AddStatusEffectGA(StatusEffectType statusEffectType, int stackCount, List<CombatantView> targets, HeroView caster)
    {
        StatusEffectType = statusEffectType;
        StackCount = stackCount;
        Targets = targets;
        Caster = caster;
    }
}
