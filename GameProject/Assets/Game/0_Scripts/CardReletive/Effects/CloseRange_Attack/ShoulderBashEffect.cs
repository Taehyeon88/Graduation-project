using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShoulderBashEffect : Effect, IUseCustomRangeVG
{
    [SerializeField] private int distance;
    [SerializeField] private int attackDistance;
    [SerializeField] private int damage;

    public Action<int, List<Vector2Int>> GetCustomRangeVGEvent()
    {
        return (owner, range) => PlayerCardEffectSystem.Instance.ShoulderBashRVG(owner, range);
    }

    public override GameAction GetGameAction(EffectInfo effectInfo)
    {
        ShoulderBashGA shoulderBashGA = new(distance, attackDistance, CalculateDamage(damage), effectInfo.targetPoses);
        InitDamageRate();
        return shoulderBashGA;
    }
}
