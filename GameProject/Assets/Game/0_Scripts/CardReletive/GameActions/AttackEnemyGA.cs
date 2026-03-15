using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackEnemyGA : GameAction
{
    public List<Vector2Int> TargetPoses { get; private set; }
    public int Amount { get; private set; }

    public AttackEnemyGA(List<Vector2Int> targetPoses, int amount)
    {
        this.TargetPoses = targetPoses;
        this.Amount = amount;
    }
}
