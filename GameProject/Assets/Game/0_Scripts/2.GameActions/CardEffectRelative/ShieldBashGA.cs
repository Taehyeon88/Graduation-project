using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldBashGA : GameAction, IHaveCaster
{
    public float Amount { get; private set; }
    public List<Vector2Int> TargetPoses { get; private set; }
    public HeroView Caster { get; private set; }
    Token IHaveCaster.Caster => Caster;
    public ShieldBashGA(float amount, List<Vector2Int> targetPoses, HeroView myView)
    {
        Amount = amount;
        TargetPoses = targetPoses;
        this.Caster = myView;
    }
}
