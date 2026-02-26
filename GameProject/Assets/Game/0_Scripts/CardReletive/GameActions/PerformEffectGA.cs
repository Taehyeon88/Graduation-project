using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerformEffectGA : GameAction
{
    public Effect Effect { get; set; }
    public List<CombatantView> Targets { get; set; }
    public CombatantView Caster { get; set; }
    public PerformEffectGA(Effect effect, List<CombatantView> targets, CombatantView caster)
    {
        Effect = effect;
        Targets = targets == null ? null : new(targets);
        Caster = caster;
    }
    public PerformEffectGA(Effect effect, CombatantView target, CombatantView caster)
    {
        Effect = effect;
        Targets = new() { target };
        Caster = caster;
    }
}
