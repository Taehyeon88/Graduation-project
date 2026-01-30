using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddSPDGA : GameAction
{
    public int Amount { get; private set; }
    public AddSPDGA(int amount)
    {
        Amount = amount;
    }
}
