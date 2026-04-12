using System;
using UnityEngine;

public class Slime_AttackEA : EnemyAction
{
    public override Sprite Icon
    {
        get { return icon; }
        protected set {}
    }
    public override string Description
    {
        get { return description; }
        protected set {}
    }

    [SerializeField] private int damage = 6;
    [SerializeField] private string description;
    [SerializeField] private Sprite icon;

    public override void PlayEnemyAction(EnemyView enemy)
    {
        var poses = EnemyRM.GetGridRanges(TokenSystem.Instance.GetTokenPosition(enemy), ActDistance, IsPenetration);
        AttackHeroGA attackHeroGA = new(enemy, damage, poses);
        ActionSystem.Instance.AddReaction(attackHeroGA);
    }

    public override EnemyAction Clone()
    {
        return new Slime_AttackEA()
        {
            icon = icon,
            description = description,
            damage = damage,
        };
    }
}
