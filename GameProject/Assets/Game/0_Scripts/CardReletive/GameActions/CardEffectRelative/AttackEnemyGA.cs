using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackEnemyGA : GameAction
{
    public List<Vector2Int> TargetPoses { get; private set; }
    public float Amount { get; private set; }
    public bool IsRandomTargetMode { get; private set; }

    public AttackEnemyGA(List<Vector2Int> targetPoses, float amount, bool isRandomTargetMode = false)
    {
        this.TargetPoses = targetPoses;
        this.Amount = amount;
        IsRandomTargetMode = isRandomTargetMode;
    }
}
