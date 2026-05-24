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
        int distance = ReservedEA.ActDistance;
        bool penetration = ReservedEA.IsPenetration;

        var range = enemyRM.GetGridRanges(enemyPos, distance, penetration);

        if (range.Contains(HeroSystem.Instance.HeroPosition))
        {
            //행동 상태로 변경
            enemy.SetNextAction(ReservedEA);
            //방향 설정
            Vector2Int dir = HeroSystem.Instance.HeroPosition - enemyPos;
            enemy.NextAction.Directions.Clear();
            enemy.NextAction.Directions.Add(dir);
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
