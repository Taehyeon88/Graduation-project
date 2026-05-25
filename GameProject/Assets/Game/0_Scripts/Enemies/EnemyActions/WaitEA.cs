using DG.Tweening;
using UnityEngine;

public class WaitEA : EnemyAction
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

    public EnemyAction ReservedEA { get; set; }
    [SerializeField] private string description;
    [SerializeField] private Sprite icon;

    public void ShouldStopWaiting(EnemyView enemy)
    {
        Vector2Int enemyPos = TokenSystem.Instance.GetTokenPosition(enemy);
        EnemyRangeMode enemyRM = ReservedEA.EnemyRM;
        EnemyTargetMode enemyTM = ReservedEA.EnemyTM;
        int distance = ReservedEA.ActDistance;

        var range = enemyRM.GetGridRanges(enemyPos, distance);

        if (range.Contains(HeroSystem.Instance.HeroPosition))
        {
            //행동 상태로 변경
            enemy.SetNextAction(ReservedEA);
            //방향 설정
            var dirs = enemyTM.GetDirections(range, HeroSystem.Instance.HeroPosition, enemyPos, distance);
            enemy.NextAction.Directions = dirs;
            //공격 범위 그리기
            enemy.Enemy.SetDrawActActionVG(true, enemy, enemy.NextAction);
        }
    }

    public override Sequence PlayEnemyAction(EnemyView enemy)
    {
        return null;
    }

    public override EnemyAction Clone()
    {
        return new WaitEA()
        {
            icon = icon,
            description = description,
            ReservedEA = ReservedEA
        };
    }
}
