using System.Collections.Generic;

public class HeroPromotion   // 영웅 종류 1개의 승급 상태 (GameSystem이 HeroData별로 보관, 같은 종류의 Hero들이 함께 참조)
{
    public int Level { get; private set; }         //승급 단계 (0부터)
    public int HealthBonus { get; private set; }   //승급으로 얻은 체력 합계

    public IReadOnlyList<TraitData> Traits   //현재 특성 칸 (빈 칸 제외, 칸 순서 유지)
    {
        get
        {
            List<TraitData> result = new(slotCount);
            for (int i = 0; i < slotCount; i++)
            {
                TraitData trait = GetSlotTrait(i);
                if (trait != null) result.Add(trait);
            }
            return result;
        }
    }

    private readonly HeroData data;
    private readonly int slotCount = CombatantView.MaxTraitCount;   //특성 칸 수 (전투 규칙의 유닛당 최대 특성 수)
    private readonly TraitData[] baseTraits;   //칸별 원래 특성 (강화 전)
    private readonly bool[] enhanced;          //칸별 강화 여부 (특성마다 1회)

    public HeroPromotion(HeroData data)
    {
        this.data = data;
        baseTraits = new TraitData[slotCount];
        enhanced = new bool[slotCount];

        //초기 칸 = 고유 특성
        if (data.Traits == null) return;
        int slot = 0;
        foreach (TraitData trait in data.Traits)
        {
            if (trait == null) continue;
            if (slot >= slotCount) break;
            baseTraits[slot++] = trait;
        }
    }

    public TraitData GetSlotTrait(int index)
    {
        TraitData trait = baseTraits[index];
        if (trait == null) return null;
        return enhanced[index] ? trait.Enhanced : trait;
    }

    public bool IsEnhanced(int index) => enhanced[index];

    //고를 수 있는 선택지: 빈 칸이 있으면 아직 없는 승급 특성마다 새 특성, 강화형이 있고 강화 안 한 칸마다 강화
    public List<PromotionOption> GetOptions()
    {
        List<PromotionOption> options = new();

        int emptySlot = System.Array.IndexOf(baseTraits, null);
        if (emptySlot >= 0 && data.PromotionTraits != null)
        {
            foreach (TraitData trait in data.PromotionTraits)
            {
                if (trait == null || System.Array.IndexOf(baseTraits, trait) >= 0) continue;
                options.Add(new PromotionOption(false, emptySlot, null, trait));
            }
        }

        for (int i = 0; i < slotCount; i++)
        {
            if (baseTraits[i] == null || enhanced[i] || baseTraits[i].Enhanced == null) continue;
            options.Add(new PromotionOption(true, i, baseTraits[i], baseTraits[i].Enhanced));
        }

        return options;
    }

    //선택지 적용 후 단계 +1 (비용·단계 상한 검사는 PromotionSystem)
    public void Apply(PromotionOption option, int healthGain)
    {
        if (option.IsEnhance)
            enhanced[option.SlotIndex] = true;
        else
            baseTraits[option.SlotIndex] = option.After;

        Level++;
        HealthBonus += healthGain;
    }
}
