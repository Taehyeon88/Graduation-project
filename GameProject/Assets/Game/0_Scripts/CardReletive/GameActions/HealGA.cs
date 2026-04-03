using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealGA : GameAction
{
    public float Amount { get; private set; }
    public List<Vector2Int> TargetPoses { get; private set; }
    public List<CombatantView> Targets { get; private set; }

    //그리드 기반
    public HealGA(float amount, List<Vector2Int> targetPoses)
    {
        Amount = amount;
        TargetPoses = targetPoses;
        Targets = new();
    }

    //대상 기반
    public HealGA(float amount, List<CombatantView> target)
    {
        Amount = amount;
        Targets = target;
    }
}
