using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldBashGA : GameAction
{
    public float Amount { get; private set; }
    public List<Vector2Int> TargetPoses { get; private set; }
    public ShieldBashGA(float amount, List<Vector2Int> targetPoses)
    {
        Amount = amount;
        TargetPoses = targetPoses;
    }
}
