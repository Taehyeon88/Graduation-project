using System.Text;
using UnityEngine;

public class Card
{
    public Hero Unit => hero;
    public int APCost => Mathf.Max(0, hero.Data.APCost + (hero.Rumor?.Rumor is IAPCostModifier modifier ? modifier.APCostBonus : 0));
    public bool CanPlaceOnNeutral => hero.Rumor?.Rumor is INeutralPlacement placement && placement.CanPlaceOnNeutral;
    public string Title => hero.Data.Name;
    public Sprite Image => hero.Data.Sprite;

    //스탯 + 특성 자동 조합 설명 (HeroData에 설명 필드가 없어 조합)
    public string Description
    {
        get
        {
            StringBuilder sb = new();
            sb.Append($"HP {hero.MaxHealth}  공격 {hero.Damage}  속도 {hero.Speed}");
            foreach (TraitData trait in hero.Traits)
            {
                if (trait == null) continue;
                sb.Append($"\n{trait.name}: {trait.Description}");
            }
            if (hero.Rumor != null) sb.Append($"\n{hero.Rumor.Title}: {hero.Rumor.Description}");
            return sb.ToString();
        }
    }

    private readonly Hero hero;

    public Card(Hero hero)
    {
        this.hero = hero;
    }
}
