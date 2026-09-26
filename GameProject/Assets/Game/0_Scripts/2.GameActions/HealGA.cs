using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealGA : GameAction, IHaveCaster
{
    public float Amount { get; private set; }
    public List<Vector2Int> TargetPoses { get; private set; }
    public List<CombatantView> Targets { get; private set; } = new(10);
    public HeroView Caster { get; private set; }
    Token IHaveCaster.Caster => Caster;

    public HealGA(float amount, List<Vector2Int> targetPoses, HeroView caster)
    {
        Amount = amount;
        TargetPoses = targetPoses;
        Caster = caster;
    }
}
