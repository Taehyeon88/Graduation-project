using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealGA : GameAction
{
    public float Amount { get; private set; }
    public List<Vector2Int> TargetPoses { get; private set; }
    public List<CombatantView> Targets { get; private set; } = new(10);

    public HealGA(float amount, List<Vector2Int> targetPoses)
    {
        Amount = amount;
        TargetPoses = targetPoses;
    }
}
