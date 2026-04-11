using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveGA : GameAction
{
    public CombatantView mover { get; private set; }
    public Vector2Int movePosition { get; private set; }
    public bool isKnockBack { get; private set; }
    public MoveGA(CombatantView mover, Vector2Int movePosition, bool isKnockBack = false)
    {
        this.mover = mover;
        this.movePosition = movePosition;
        this.isKnockBack = isKnockBack;
    }
}
