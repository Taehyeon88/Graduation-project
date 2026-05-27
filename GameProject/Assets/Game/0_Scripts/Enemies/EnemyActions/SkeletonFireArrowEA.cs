using DG.Tweening;
using IsoTools;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonFireArrowEA : EnemyAction
{
    public override Sprite Icon
    {
        get { return icon; }
        protected set { }
    }
    public override string Description
    {
        get { return description; }
        protected set { }
    }

    [SerializeField] private int damage = 6;
    [SerializeField] private string description;
    [SerializeField] private Sprite icon;

    private IsoObject arrow;

    public override Sequence PlayEnemyAction(EnemyView enemy)
    {
        Sequence sequence = DOTween.Sequence();

        Vector2Int currentPos = TokenSystem.Instance.GetTokenPosition(enemy);
        Vector2Int targetPos = TokenSystem.Instance.GetPositionByDirection(enemy, Directions[0]);
        Vector2Int direction = Directions[0];

        if (arrow == null)
        {
            arrow = ProjectionCreator.Instance.CreateProjection(
                ProjectionType.SkeletonArrow, 
                enemy.transform.position, 
                TokenSystem.Instance.IsoWorld.transform
                );
        }
        else
        {
            arrow.gameObject.SetActive(true);
        }

        Tween startTween = Utility.GetTween(enemy, currentPos, -direction, 0.2f, 0.1f, Ease.OutCubic);
        Tween returnTween = Utility.GetTween(enemy, currentPos, 0.1f);
        Tween projectionTween = Utility.GetArrowBezierTween(
            arrow,
            arrow.transform.GetChild(0).transform,
            new(currentPos.x, currentPos.y, 1),
            new(targetPos.x, targetPos.y, 1),
            0.3f,
            Ease.Linear
            );

        projectionTween.OnStart(() =>
        {
            SoundSystem.Instance.PlaySound(8);
        });
        projectionTween.OnComplete(() =>
        {
            arrow.gameObject.SetActive(false);
            //꽂히는 사운드 실행 SoundSystem.PlaySound();
        });

        sequence.Append(startTween).Join(projectionTween).Join(returnTween);

        AttackHeroGA attackHeroGA = new(enemy, damage, new() { targetPos });
        ActionSystem.Instance.AddReaction(attackHeroGA);

        return sequence;
    }

    public override EnemyAction Clone()
    {
        return new SkeletonFireArrowEA()
        {
            icon = icon,
            description = description,
            damage = damage,
        };
    }
}
