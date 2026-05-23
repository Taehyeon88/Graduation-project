using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackEnemyGA : GameAction
{
    public List<Vector2Int> TargetPoses { get; private set; }
    public float Amount { get; private set; }

    public AttackEnemyGA(List<Vector2Int> targetPoses, float amount)
    {
        this.TargetPoses = targetPoses;
        this.Amount = amount;
    }
}
