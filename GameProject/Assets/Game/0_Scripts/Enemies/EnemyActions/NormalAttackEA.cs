using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public class NormalAttackEA : EnemyAction
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

    public override Sequence PlayEnemyAction(EnemyView enemy)
    {
        var poses = new List<Vector2Int>();
        foreach (var dir in Directions)
            poses.Add(TokenSystem.Instance.GetDirectionPos(enemy, dir));

        var curPos = TokenSystem.Instance.GetTokenPosition(enemy);
        Tween attackTween = Utility.GetTween(enemy, poses[0], 0.15f);
        Tween backTween = Utility.GetTween(enemy, curPos, 0.25f);

        Sequence squ = DOTween.Sequence();
        squ.Append(attackTween).Append(backTween);

        AttackHeroGA attackHeroGA = new(enemy, damage, poses); 
        ActionSystem.Instance.AddReaction(attackHeroGA);

        return squ;
    }

    public override EnemyAction Clone()
    {
        return new NormalAttackEA()
        {
            icon = icon,
            description = description,
            damage = damage,
        };
    }
}
