using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExecutionGA : GameAction
{
    public int Amount { get; set; }
    public List<Vector2Int> TargetPoses { get; private set; }
    public ExecutionGA(int amount, List<Vector2Int> targetPoses)
    {
        Amount = amount;
        TargetPoses = targetPoses;
    }
}
