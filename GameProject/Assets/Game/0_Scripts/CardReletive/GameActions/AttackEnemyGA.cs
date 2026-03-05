using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackEnemyGA : GameAction
{
    public GridTargetMode GridTargetMode { get; private set; }
    public int Amount { get; private set; }

    public AttackEnemyGA(GridTargetMode gridTargetMode, int amount)
    {
        this.GridTargetMode = gridTargetMode;
        this.Amount = amount;
    }
}
