using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AttackEnemyEffect : Effect
{
    [SerializeField] private float amount;
    public override GameAction GetGameAction(EffectInfo effectInfo)
    {
        AttackEnemyGA attackEnemyGA = new(effectInfo.targetPoses, amount);
        return attackEnemyGA;
    }
}
