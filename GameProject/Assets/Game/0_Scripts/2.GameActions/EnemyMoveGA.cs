using UnityEngine;

public class EnemyMoveGA : GameAction
{
    public EnemyView EnemyView {  get; private set; }
    public bool IsIsolation { get; private set; }
    public EnemyMoveGA(EnemyView enemyView, bool isIsolation = false)
    {
        EnemyView = enemyView;
        IsIsolation = isIsolation;
    }
}
