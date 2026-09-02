using UnityEngine;

public class TurnGA : GameAction
{
    public TurnType Type { get; private set; }

    public TurnGA(TurnType type)
    {
        Type = type;
    }
}
