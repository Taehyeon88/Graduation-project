public class PlayEnemyEAGA : GameAction
{
    public EnemyView Enemy { get; private set; }
    public EnemyAction Action { get; private set; }

    public PlayEnemyEAGA(EnemyView enemy, EnemyAction action)
    {
        Enemy = enemy;
        Action = action;
    }
}
