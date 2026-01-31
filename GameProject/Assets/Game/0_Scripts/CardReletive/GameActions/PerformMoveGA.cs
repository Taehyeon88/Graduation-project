using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerformMoveGA : GameAction
{
    public CombatantView mover { get; private set; }
    public List<Vector2Int> path { get; private set; }
    public PerformMoveGA(CombatantView mover, List<Vector2Int> path)
    {
        this.mover = mover;
        this.path = path;
    }
}
