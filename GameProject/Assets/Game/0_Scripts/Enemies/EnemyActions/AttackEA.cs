using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public class AttackEA : EnemyAction
{
    public override Sprite Icon
    {
        get { return icon; }
        protected set {}
    }
    public override string Description
    {
        get { return $"인접 {Distance}칸 내, 영웅에게 {Damage} 피해의 공격"; }
        protected set {}
    }
    public override string TextInfo
    {
        get { return Damage.ToString(); }
        protected set { }
    }

    [field: SerializeField] public int Damage { get; protected set; } = 6;
    [field: SerializeField] public int Distance { get; protected set; } = 1;

    [SerializeField] private Sprite icon;

    public override Sequence PlayEnemyAction(EnemyView enemy)
    {
        bool singleTarget = targetRange == null || targetRange.Count <= 0;
        Vector2Int tweenPos = singleTarget? targetPosition : targetRange[targetRange.Count/2];

        var curPos = TokenSystem.Instance.API.GetTokenPosition(enemy);
        Tween attackTween = Utility.GetTween(enemy, tweenPos, 0.8f, 0.15f, Ease.Unset);
        Tween backTween = Utility.GetBackTween(enemy, 0.25f);

        Sequence squ = DOTween.Sequence();
        squ.Append(attackTween);

        AttackHeroGA attackHeroGA = singleTarget ?
                   new(enemy, Damage, targetPosition) : new(enemy, Damage, targetRange);
        ActionSystem.Instance.AddReaction(attackHeroGA);

        DOAnimationGA animationGA = new(backTween);
        ActionSystem.Instance.AddReaction(animationGA);

        return squ;
    }

    public override EnemyAction Clone()
    {
        return new AttackEA()
        {
            icon = icon,
            Description = Description,
            Damage = Damage,
            Distance = Distance,
            TextInfo = TextInfo,
        };
    }
}
