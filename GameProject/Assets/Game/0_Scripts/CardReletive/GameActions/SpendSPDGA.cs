using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpendSPDGA : GameAction
{
    public int Amount { get; private set; }
    public SpendSPDGA(int amount)
    {
        Amount = amount;
    }
}
