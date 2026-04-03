using System.Collections;
using System.Collections.Generic;
using SerializeReferenceEditor;
using UnityEngine;

public class SplashGA : GameAction
{
    public CombatantView Caster { get; private set; }
    public List<Vector2Int> TargetPoses { get; private set; }
    public GridRangeMode GridRangeMode { get; private set; }
    public bool IsPentration { get; private set; }
    public int Distance { get; private set; }
    public float Damage { get; private set; }
    public float SplashDamage { get; private set; }
    public SplashGA(List<Vector2Int> targetPoses, GridRangeMode gridRangeMode, bool isPentration, int distance, float damage, float splashDamage, CombatantView caster)
    {
        TargetPoses = targetPoses;
        GridRangeMode = gridRangeMode;
        IsPentration = isPentration;
        Distance = distance;
        Damage = damage;
        SplashDamage = splashDamage;
        Caster = caster;
    }
}
