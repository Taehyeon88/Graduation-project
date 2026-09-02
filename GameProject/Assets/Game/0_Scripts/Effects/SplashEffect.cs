using System;
using System.Collections;
using System.Collections.Generic;
using SerializeReferenceEditor;
using UnityEngine;

[System.Serializable]
public class SplashEffect : Effect
{
    [SerializeField] private float damage;
    [SerializeField] private float splashDamage;
    [SerializeReference, SR] private RangeMode gridRangeMode;
    [SerializeField] private int distance;
    [SerializeField] private bool isPentration;

    public RangeMode GridRangeMode { get { return gridRangeMode; } }
    public int Distance { get { return distance; } }
    public bool IsPentration {  get { return isPentration; } }

    public override GameAction GetGameAction(List<Vector2Int> targetpoes, HeroView myView)
    {
        var shplash = new SplashGA(
                  targetpoes,
                  gridRangeMode,
                  isPentration,
                  distance,
                  damage,
                  splashDamage,
                  myView
             );
        return shplash;
    }
}
