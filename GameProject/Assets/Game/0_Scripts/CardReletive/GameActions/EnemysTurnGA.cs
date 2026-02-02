using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemysTurnGA : GameAction
{
    public bool isStartGame { get; private set; }
    public EnemysTurnGA(bool isStartGame = false)
    {
        this.isStartGame = isStartGame;
    }
}
