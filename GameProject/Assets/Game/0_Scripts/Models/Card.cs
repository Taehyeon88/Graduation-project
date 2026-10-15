using UnityEngine;

public class Card
{
    public HeroData Unit => data;
    public int APCost => data.APCost;
    public string Title => data.Name;
    public Sprite Image => data.simbolIcon;

    public readonly HeroData data;

    public Card(HeroData data)
    {
        this.data = data;
    }
}
