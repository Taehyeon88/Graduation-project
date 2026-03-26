using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackEnemyEffect : Effect
{
    [SerializeField] private int amount;
    public override GameAction GetGameAction(EffectInfo effectInfo)
    {
        AttackEnemyGA attackEnemyGA = new(effectInfo.targetPoses, CalculateDamage(amount));
        InitDamageRate();
        return attackEnemyGA;
    }
}
