using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveGA : GameAction
{
    public GridTargetMode GridTargetMode { get; private set; }
    public int Distance { get; private set; }
    public bool IsFirstMove { get; private set; }

    public PlayerMoveGA(GridTargetMode gridTargetMode, int distance, bool isFirstMove = false)
    {
        this.GridTargetMode = gridTargetMode;
        this.Distance = distance;
        this.IsFirstMove = isFirstMove;
    }
}
