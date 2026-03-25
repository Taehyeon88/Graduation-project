using System.Collections;
using System.Collections.Generic;
using SerializeReferenceEditor;
using UnityEngine;

public class SplashEffect : Effect
{
    [SerializeField] private int damage;
    [SerializeField] private int splashDamage;
    [SerializeReference, SR] private GridRangeMode gridRangeMode;
    [SerializeField] private int distance;
    [SerializeField] private bool isPentration;

    public override GameAction GetGameAction(EffectInfo effectInfo)
    {
        return new SplashGA(effectInfo.targetPoses, gridRangeMode, isPentration, distance, damage, splashDamage, effectInfo.caster);
    }
}
