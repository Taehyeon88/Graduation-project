using UnityEngine;

public class AddSpdGA : GameAction
{
    public int Amount { get; set; }
    public AddSpdGA(int amount = 1)
    {
        Amount = amount;
    }
}
