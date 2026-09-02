using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldBashGA : GameAction
{
    public float Amount { get; private set; }
    public List<Vector2Int> TargetPoses { get; private set; }
    public HeroView myView { get; private set; }
    public ShieldBashGA(float amount, List<Vector2Int> targetPoses, HeroView myView)
    {
        Amount = amount;
        TargetPoses = targetPoses;
        this.myView = myView;
    }
}
