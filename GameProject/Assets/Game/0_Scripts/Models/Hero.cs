using System.Collections.Generic;
using System.Linq;

public class Hero   // 덱에 든 영웅 한 장의 런타임 상태 (HeroData = 불변 정의, 스테이지를 거치며 바뀌는 값은 여기에)
{
    public readonly HeroData Data;

    public int HealthBonus { get; private set; }
    public int DamageBonus { get; private set; }
    public int SpeedBonus { get; private set; }
    public int CriticalBonus { get; private set; }

    public RumorData Rumor { get; private set; }   //스테이지 중 획득한 소문 (영웅당 1개, 시작 소문 없음)

    public int MaxHealth => Data.Health + HealthBonus + (promotion?.HealthBonus ?? 0);
    public int Damage => Data.Damage + DamageBonus;
    public int Speed => Data.Speed + SpeedBonus;
    public int Critical => Data.Critical + CriticalBonus;
    public IReadOnlyList<TraitData> Traits   //승급 상태가 있으면 그 특성 칸, 없으면(기본 제공 카드 등) 데이터의 특성
        => promotion != null ? promotion.Traits : (Data.Traits ?? Enumerable.Empty<TraitData>()).ToList();

    private readonly HeroPromotion promotion;   //보유 영웅만 가짐 (같은 종류끼리 공유, null = 승급 미적용)

    public Hero(HeroData data, HeroPromotion promotion = null)
    {
        Data = data;
        this.promotion = promotion;
    }

    public bool CanAddRumor => Rumor == null;

    public void AddHealth(int amount) => HealthBonus += amount;

    public void AddRumor(RumorData rumor)
    {
        if (rumor != null) Rumor = rumor;
    }
}
