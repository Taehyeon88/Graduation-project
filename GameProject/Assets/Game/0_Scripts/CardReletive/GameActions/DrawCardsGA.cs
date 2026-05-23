using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawCardsGA : GameAction
{
    public int Amount { get; set; }
    public bool IsFirstDraw { get; set; }
    public DrawCardsGA(int amount, bool isFirstDraw = false)
    {
        Amount = amount;
        IsFirstDraw = isFirstDraw;
    }
}
