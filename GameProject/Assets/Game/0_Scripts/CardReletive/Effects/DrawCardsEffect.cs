using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DrawCardsEffect : Effect
{
    [SerializeField] private int drawAmount;
    public override GameAction GetGameAction(EffectInfo effectInfo)
    {
        DrawCardsGA drawCardsGA = new(drawAmount);
        return drawCardsGA;
    }
}
