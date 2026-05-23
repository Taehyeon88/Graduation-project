using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawCardFromDiscardPileGA : GameAction
{
    public Card Card { get; private set; }
    public DrawCardFromDiscardPileGA(Card card)
    {
        Card = card;
    }
}
