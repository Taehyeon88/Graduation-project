using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShoulderBashGA : GameAction
{
    public int Distance { get; private set; }
    public int AttackDistance { get; private set; }
    public int Damage { get; private set; }
    public List<Vector2Int> TargetPoses { get; private set; }

    public ShoulderBashGA(int distance, int attackDistance, int damage, List<Vector2Int> targetPoses)
    {
        this.Distance = distance;
        this.AttackDistance = attackDistance;
        this.Damage = damage;
        this.TargetPoses = targetPoses;
    }
}
