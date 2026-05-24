using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayCardTargetingGA : GameAction
{
    public Card Card { get; private set; }
    public Action EndSelectAction { get; private set; }
    public PlayCardTargetingGA(Card card, Action endSelectAction)
    {
        Card = card;
        EndSelectAction = endSelectAction;
    }
}
