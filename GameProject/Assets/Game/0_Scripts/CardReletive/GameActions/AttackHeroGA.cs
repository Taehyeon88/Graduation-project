using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackHeroGA : GameAction, IHaveCaster
{
    public EnemyView Attacker { get; private set; }

    public CombatantView Caster { get; private set; }
    public int DamageAmount { get; private set; }
    public Vector2Int[] AttackArea { get; private set; }

    public AttackHeroGA(EnemyView attacker, int damageAmount, Vector2Int[] attackArea)
    {
        Attacker = attacker;
        Caster = attacker;
        DamageAmount = damageAmount;
        AttackArea = attackArea;
    }
}
