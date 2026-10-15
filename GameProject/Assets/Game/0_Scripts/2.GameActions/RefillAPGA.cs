using UnityEngine;

public class RefillAPGA : GameAction
{
    public int Amount { get; private set; }   //획득할 AP (최대 AP를 넘는 양은 획득되지 않음)

    public RefillAPGA(int amount)
    {
        Amount = amount;
    }
}
