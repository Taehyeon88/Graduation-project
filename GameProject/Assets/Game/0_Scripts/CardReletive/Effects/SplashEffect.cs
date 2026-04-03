using System;
using System.Collections;
using System.Collections.Generic;
using SerializeReferenceEditor;
using UnityEngine;

public class SplashEffect : Effect, IUseCustomTargetVG
{
    [SerializeField] private float damage;
    [SerializeField] private float splashDamage;
    [SerializeReference, SR] private GridRangeMode gridRangeMode;
    [SerializeField] private int distance;
    [SerializeField] private bool isPentration;

    public GridRangeMode GridRangeMode { get { return gridRangeMode; } }
    public int Distance { get { return distance; } }
    public bool IsPentration {  get { return isPentration; } }

    public Action<bool, int, List<Vector2Int>, Card> GetCustomTargetVGEvent()
    {
        return (ac, ow, ra, ca) => PlayerCardEffectSystem.Instance.SplashTVG(ac, ow, ra, ca);
    }

    public override GameAction GetGameAction(EffectInfo effectInfo)
    {
        var shplash = new SplashGA(
                  effectInfo.targetPoses,
                  gridRangeMode,
                  isPentration,
                  distance,
                  damage,
                  splashDamage,
                  effectInfo.caster
             );
        return shplash;
    }
}
