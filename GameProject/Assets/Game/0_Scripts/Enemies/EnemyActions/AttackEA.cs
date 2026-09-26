using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public class AttackEA : EnemyAction, IHaveDamage, IHaveDistance
{
    public override Sprite Icon
    {
        get { return icon; }
        protected set {}
    }
    public override string Description
    {
        get { return $"인접 {Distance}칸 내, 영웅에게 {Damage_Amount} 피해의 공격"; }
        protected set {}
    }
    public override string TextInfo
    {
        get { return Damage_Amount.ToString(); }
        protected set { }
    }

    [field: SerializeField] public float Damage_Amount { get; protected set; } = 6;
    [field: SerializeField] public int Distance { get; protected set; } = 1;


    [SerializeField] private Sprite icon;

    public override Sequence PlayEnemyAction(EnemyView enemy)
    {
        HeroView target = FindRandomHeroInRange(enemy, Distance);
        if (target == null)
            return null;   //사거리 내 대상 없음 → 무시

        targetPosition = TokenSystem.Instance.API.GetTokenPosition(target);

        bool singleTarget = targetRange == null || targetRange.Count <= 0;
        Vector2Int tweenPos = singleTarget? targetPosition : targetRange[targetRange.Count/2];

        var curPos = TokenSystem.Instance.API.GetTokenPosition(enemy);
        Tween attackTween = Utility.GetTween(enemy, tweenPos, 0.8f, 0.15f, Ease.Unset);
        Tween backTween = Utility.GetBackTween(enemy, 0.25f);

        Sequence squ = DOTween.Sequence();
        squ.Append(attackTween);

        AttackHeroGA attackHeroGA = singleTarget ?
                   new(enemy, Damage_Amount, targetPosition) : new(enemy, Damage_Amount, targetRange);
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
            Damage_Amount = Damage_Amount,
            Distance = Distance,
            TextInfo = TextInfo,
        };
    }
}
