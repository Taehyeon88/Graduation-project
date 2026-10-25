using System.Collections.Generic;
using System.Linq;

public class Hero   // 덱에 든 영웅 한 장의 런타임 상태 (HeroData = 불변 정의, 스테이지를 거치며 바뀌는 값은 여기에)
{
    public readonly HeroData Data;

    public int HealthBonus { get; private set; }
    public int DamageBonus { get; private set; }
    public int SpeedBonus { get; private set; }
    public int CriticalBonus { get; private set; }
    public IReadOnlyList<TraitData> AcquiredTraits => acquiredTraits;   //스테이지 중 획득한 특성

    public RumorData Rumor { get; private set; }   //스테이지 중 획득한 소문 (영웅당 1개, 시작 소문 없음)

    public int MaxHealth => Data.Health + HealthBonus;
    public int Damage => Data.Damage + DamageBonus;
    public int Speed => Data.Speed + SpeedBonus;
    public int Critical => Data.Critical + CriticalBonus;
    public IReadOnlyList<TraitData> Traits   //기본 특성 + 획득 특성
        => (Data.Traits ?? Enumerable.Empty<TraitData>()).Concat(acquiredTraits).ToList();

    private readonly List<TraitData> acquiredTraits = new();

    public Hero(HeroData data)
    {
        Data = data;
    }

    public bool CanAddTrait => Traits.Count < CombatantView.MaxTraitCount;
    public bool CanAddRumor => Rumor == null;

    public void AddHealth(int amount) => HealthBonus += amount;

    public void AddTrait(TraitData trait)
    {
        if (trait != null) acquiredTraits.Add(trait);
    }

    public void AddRumor(RumorData rumor)
    {
        if (rumor != null) Rumor = rumor;
    }
}
