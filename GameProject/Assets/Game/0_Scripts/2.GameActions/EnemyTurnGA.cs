using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTurnGA : GameAction
{
    public EnemyView EnemyView { get; private set; }
    public EnemyTurnGA(EnemyView enemyView)
    {
        EnemyView = enemyView;
    }
}
