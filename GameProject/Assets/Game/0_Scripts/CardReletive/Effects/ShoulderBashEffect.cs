using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShoulderBashEffect : Effect
{
    [SerializeField] private int distance;
    [SerializeField] private int attackDistance;
    [SerializeField] private int damage;
    public override GameAction GetGameAction(EffectInfo effectInfo)
    {
        ShoulderBashGA shoulderBashGA = new(distance, attackDistance, damage, effectInfo.gridTargetMode);
        return shoulderBashGA;
    }
}
