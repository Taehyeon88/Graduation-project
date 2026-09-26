using DG.Tweening;
using UnityEngine;

public class FireArrowEA : EnemyAction, IHaveDamage, IHaveDistance
{
    public override Sprite Icon
    {
        get { return icon; }
        protected set { }
    }
    public override string Description
    {
        get { return $"사거리 {Distance}칸 내, 영웅에게 {Damage_Amount} 피해의 원거리 공격"; }
        protected set { }
    }
    public override string TextInfo
    {
        get { return Damage_Amount.ToString(); }
        protected set { }
    }

    [field: SerializeField] public float Damage_Amount { get; protected set; } = 4;
    [field: SerializeField] public int Distance { get; protected set; } = 3;

    [SerializeField] private float arrowDuration = 0.35f;
    [SerializeField] private Sprite icon;

    public override Sequence PlayEnemyAction(EnemyView enemy)
    {
        HeroView target = FindRandomHeroInRange(enemy, Distance);
        if (target == null)
            return null;   //사거리 내 대상 없음 → 무시

        targetPosition = TokenSystem.Instance.API.GetTokenPosition(target);

        var curPos = TokenSystem.Instance.API.GetTokenPosition(enemy);

        Tween arrowTween = ProjectileSystem.Instance.PlayArrow(
            Utility.Vector2IntToVector3(curPos, 1),
            Utility.Vector2IntToVector3(targetPosition, 1),
            arrowDuration, Ease.Linear);

        Sequence squ = DOTween.Sequence();
        squ.Append(arrowTween);

        AttackHeroGA attackHeroGA = new(enemy, Damage_Amount, targetPosition);
        ActionSystem.Instance.AddReaction(attackHeroGA);

        return squ;
    }

    public override EnemyAction Clone()
    {
        return new FireArrowEA()
        {
            icon = icon,
            Damage_Amount = Damage_Amount,
            Distance = Distance,
            arrowDuration = arrowDuration,
        };
    }
}
