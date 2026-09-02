using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AttackHeroGA : GameAction, IHaveCaster
{
    public CombatantView Caster { get; private set; }
    public float DamageAmount { get; private set; }
    public List<Vector2Int> AttackArea { get; private set; }
    public Vector2Int AttackPosition { get; private set; }

    public AttackHeroGA(EnemyView attacker, float damageAmount, List<Vector2Int> attackArea)
    {
        Caster = attacker;
        DamageAmount = damageAmount;
        AttackArea = attackArea;
    }

    public AttackHeroGA(EnemyView attacker, float damageAmount, Vector2Int attackPosition)
    {
        Caster = attacker;
        DamageAmount = damageAmount;
        AttackPosition = attackPosition;
    }
}
