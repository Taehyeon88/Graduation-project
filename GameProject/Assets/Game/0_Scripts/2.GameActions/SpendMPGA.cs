
public class SpendMPGA : GameAction
{
    public int Amount { get; set; }
    public SpendMPGA(int amount = 1)
    {
        Amount = amount;
    }
}
