using System.Collections.Generic;
using UnityEngine;

public class PlayHeroVisualEffectGA : GameAction
{
    public int EffectId { get; private set; }
    public (CardType, CardSubType) CardTypes {  get; private set; }
    public int Step { get; private set; } 
    public List<Vector2Int> TargetPoses { get; private set; }

    public PlayHeroVisualEffectGA((CardType, CardSubType) cardTypes, int step, List<Vector2Int> targetPoses)
    {
        CardTypes = cardTypes;
        Step = step;
        TargetPoses = new(targetPoses);
    }
}
