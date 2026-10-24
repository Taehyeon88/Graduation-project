using System.Text;
using UnityEngine;

public class Card
{
    public HeroData Unit => data;
    public int APCost => data.APCost;
    public string Title => data.Name;
    public Sprite Image => data.Sprite;

    //스탯 + 특성 자동 조합 설명 (HeroData에 설명 필드가 없어 조합)
    public string Description
    {
        get
        {
            StringBuilder sb = new();
            sb.Append($"HP {data.Health}  공격 {data.Damage}  속도 {data.Speed}");
            if (data.Traits != null)
            {
                foreach (TraitData trait in data.Traits)
                {
                    if (trait == null) continue;
                    sb.Append($"\n{trait.name}: {trait.Description}");
                }
            }
            return sb.ToString();
        }
    }

    public readonly HeroData data;

    public Card(HeroData data)
    {
        this.data = data;
    }
}
