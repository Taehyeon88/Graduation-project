//카드로 배치할 때 드는 AP를 보정하는 소문 (Card.APCost가 합산)
public interface IAPCostModifier
{
    public int APCostBonus { get; }
}
