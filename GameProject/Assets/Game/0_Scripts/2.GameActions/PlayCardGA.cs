using UnityEngine;

public class PlayCardGA : GameAction
{
    public Card Card { get; private set; }
    public Vector2Int TargetPos { get; private set; }
    public PlayCardGA(Card card, Vector2Int targetPos)
    {
        Card = card;
        TargetPos = targetPos;
    }
}
