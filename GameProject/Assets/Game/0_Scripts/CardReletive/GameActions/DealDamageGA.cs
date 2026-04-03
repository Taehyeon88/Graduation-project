using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DealDamageGA : GameAction, IHaveCaster
{
    public float Amount { get; set; }
    public List<CombatantView> Targets { get; private set; }

    public CombatantView Caster { get; private set; }
    public DamageFormulaType FormulaType { get; set; }

    public DealDamageGA(float amount, List<CombatantView> targets, CombatantView caster, DamageFormulaType formulaType = DamageFormulaType.Main)
    {
        Amount = amount;
        Targets = new(targets);
        Caster = caster;
        FormulaType = formulaType;
    }
}
