using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldBashGA : GameAction
{
    public int Amount { get; private set; }
    public List<Vector2Int> TargetPoses { get; private set; }
    public ShieldBashGA(int amount, List<Vector2Int> targetPoses)
    {
        Amount = amount;
        TargetPoses = targetPoses;
    }
}
