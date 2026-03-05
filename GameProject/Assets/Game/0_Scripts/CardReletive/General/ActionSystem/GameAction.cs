using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameAction
{
    public List<(GameAction, System.Action)> PreReactions { get; private set; } = new();
    public List<(GameAction, System.Action)> PerformReactions { get; private set; } = new();
    public List<(GameAction, System.Action)> PostReactions { get; private set; } = new();
}
