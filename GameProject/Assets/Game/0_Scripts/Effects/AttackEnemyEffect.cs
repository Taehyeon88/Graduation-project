using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AttackEnemyEffect : Effect, IHaveDamage
{
    public float Damage_Amount => amount;

    [SerializeField] private float amount;
    [SerializeField] private int count = 1;
    [SerializeField] private HeroAnimationType animationType;
    public override GameAction GetGameAction(List<Vector2Int> targetpoes, HeroView myView)
    {
        AttackEnemyGA attackEnemyGA = new(targetpoes, amount, count, myView, animationType);
        return attackEnemyGA;
    }
}
