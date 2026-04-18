using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AttackHeroGA : GameAction, IHaveCaster
{
    public EnemyView Attacker { get; private set; }

    public CombatantView Caster { get; private set; }
    public float DamageAmount { get; private set; }
    public List<Vector2Int> AttackArea { get; private set; }

    public AttackHeroGA(EnemyView attacker, float damageAmount, List<Vector2Int> attackArea)
    {
        Attacker = attacker;
        Caster = attacker;
        DamageAmount = damageAmount;
        AttackArea = attackArea;
    }
}
