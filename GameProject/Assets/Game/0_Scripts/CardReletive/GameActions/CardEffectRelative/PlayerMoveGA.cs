using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveGA : GameAction
{
    public Vector2Int TargetPos { get; private set; }

    public PlayerMoveGA(Vector2Int targetPos)
    {
        TargetPos = targetPos;
    }
}
