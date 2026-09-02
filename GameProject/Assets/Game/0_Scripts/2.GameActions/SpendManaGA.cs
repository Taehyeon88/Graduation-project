using UnityEngine;

public class SpendManaGA : GameAction
{
    public int Amount { get; set; }
    public SpendManaGA(int amount = 1)
    {
        Amount = amount;
    }
}
