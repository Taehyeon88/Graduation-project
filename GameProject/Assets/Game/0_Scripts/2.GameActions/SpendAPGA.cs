using UnityEngine;

public class SpendAPGA : GameAction
{
    public int Amount { get; set; }
    public SpendAPGA(int amount = 1)
    {
        Amount = amount;
    }
}
