using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShoulderBashEffect : Effect
{
    [SerializeField] private int distance;
    [SerializeField] private int attackDistance;
    [SerializeField] private float damage;

    public override GameAction GetGameAction(List<Vector2Int> targetpoes, HeroView myView)
    {
        ShoulderBashGA shoulderBashGA = new(distance, attackDistance, damage, targetpoes, myView);
        return shoulderBashGA;
    }
}
