using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShoulderBashGA : GameAction
{
    public int Distance { get; private set; }
    public int AttackDistance { get; private set; }
    public int Damage { get; private set; }
    public GridTargetMode GridTargetMode { get; private set; }
    public ShoulderBashGA(int distance, int attackDistance, int damage, GridTargetMode gridTargetMode)
    {
        this.Distance = distance;
        this.AttackDistance = attackDistance;
        this.Damage = damage;
        this.GridTargetMode = gridTargetMode;
    }
}
