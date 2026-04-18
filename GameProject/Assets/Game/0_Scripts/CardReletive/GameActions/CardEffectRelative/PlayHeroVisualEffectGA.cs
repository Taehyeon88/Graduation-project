using UnityEngine;

public class PlayHeroVisualEffectGA : GameAction
{
    public int EffectId { get; private set; }
    public (CardType, CardSubType) CardTypes {  get; private set; }
    public int Step { get; private set; } 
    public Token Mover { get; private set; }
    public Vector2Int CurrentPos { get; private set; }
    public Vector2Int Direction { get; private set; }

    public PlayHeroVisualEffectGA((CardType, CardSubType) cardTypes, int step, Token mover, Vector2Int currentPos, Vector2Int direction)
    {
        CardTypes = cardTypes;
        Step = step;
        Mover = mover;
        CurrentPos = currentPos;
        Direction = direction;
    }
}
