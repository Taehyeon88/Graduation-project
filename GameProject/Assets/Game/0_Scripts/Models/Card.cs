using UnityEngine;

public class Card
{
    public HeroData Unit => data.Unit;
    public int APCost => data.APCost;
    public string Title => data.Unit.Name;
    public Sprite Image => data.Unit.simbolIcon;

    public readonly CardData data;

    public Card(CardData data)
    {
        this.data = data;
    }
}
