using UnityEngine;

public class DiscardCardGA : GameAction
{
    public CardView CardView { get; private set; }
    public DiscardCardGA(CardView cardView)
    {
        CardView = cardView;
    }
}
