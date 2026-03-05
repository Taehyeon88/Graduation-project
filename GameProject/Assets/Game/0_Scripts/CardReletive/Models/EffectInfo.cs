using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct EffectInfo
{
    public List<CombatantView> targets;
    public CombatantView caster;

    //AttackEnemyGAEffect¿ë
    public GridTargetMode gridTargetMode;

    public EffectInfo(List<CombatantView> targets, CombatantView caster)
    {
        this.targets = targets;
        this.caster = caster;
        gridTargetMode = null;
    }

    public EffectInfo(GridTargetMode gridTagetMode)
    {
        this.targets = null;
        this.caster = null;
        this.gridTargetMode = gridTagetMode;
    }
}
