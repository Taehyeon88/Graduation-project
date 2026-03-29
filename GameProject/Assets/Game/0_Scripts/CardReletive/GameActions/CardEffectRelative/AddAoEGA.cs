using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddAoEGA : GameAction
{
    public CombatantView Caster { get; private set; }
    public AoEType AoEType { get; private set; }
    public List<Vector2Int> TargetPoses { get; private set; }
    public AoETargetMode AoETargetMode { get; private set; }
    public AddAoEGA(CombatantView caster, AoEType aoEType, List<Vector2Int> targetPoses, AoETargetMode aoETargetMode)
    {
        Caster = caster;
        AoEType = aoEType;
        TargetPoses = targetPoses;
        AoETargetMode = aoETargetMode;
    }
}
