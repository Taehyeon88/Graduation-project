using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AttackEnemyEffect : Effect
{
    [SerializeField] private float amount;
    [SerializeField] private int count;
    [SerializeField] private HeroAnimationType animationType;
    public override GameAction GetGameAction(List<Vector2Int> targetpoes, HeroView myView)
    {
        AttackEnemyGA attackEnemyGA = new(targetpoes, amount, count, myView, animationType);
        return attackEnemyGA;
    }
}
