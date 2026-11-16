using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PromotionSystem : Singleton<PromotionSystem>   //스테이지 선택 씬: 영웅 승급 규칙 판단·실행 (승급 상태 저장은 GameSystem)
{
    public const int MaxLevel = 2;   //승급 최대 단계 (특성 칸이 2개라 2단계로 확정 — 성장 시스템 기획서)

    [SerializeField] private int[] levelCosts = { 1, 2 };   //단계별 휘장 비용 (0번 = 승급 1)
    [SerializeField] private int healthPerLevel = 1;        //승급마다 얻는 체력 (기획서 제안값)

    public event System.Action<HeroData> Promoted;   //승급이 끝났을 때 (UI 갱신용)

    private GameSystem Game => GameSystem.Instance;

    //Publics
    //보유 영웅 종류 목록 (같은 영웅 카드가 여러 장이어도 1개, 덱 순서 유지)
    public List<HeroData> GetOwnedHeroes()
        => Game.Deck.Select(hero => hero.Data).Distinct().ToList();

    public int GetLevel(HeroData heroData) => Game.GetPromotion(heroData).Level;

    public bool IsMaxLevel(HeroData heroData) => GetLevel(heroData) >= MaxLevel;

    //다음 단계 휘장 비용 (최대 단계면 0)
    public int GetCost(HeroData heroData)
    {
        int level = GetLevel(heroData);
        return level < MaxLevel ? levelCosts[level] : 0;
    }

    public List<PromotionOption> GetOptions(HeroData heroData)
        => IsMaxLevel(heroData) ? new List<PromotionOption>() : Game.GetPromotion(heroData).GetOptions();

    //보유 영웅 + 다음 단계 있음 + 휘장 충분 + 선택지 1개 이상
    public bool CanPromote(HeroData heroData)
    {
        if (heroData == null || !IsOwned(heroData) || IsMaxLevel(heroData)) return false;
        if (Game.Insignias < GetCost(heroData)) return false;
        return GetOptions(heroData).Count > 0;
    }

    //선택지로 승급 (되돌릴 수 없음). 선택지가 지금 고를 수 있는 것이 아니면 실패
    public bool Promote(HeroData heroData, PromotionOption option)
    {
        if (!CanPromote(heroData) || option == null) return false;
        if (!GetOptions(heroData).Any(o => IsSameOption(o, option))) return false;
        if (!Game.TrySpendInsignias(GetCost(heroData))) return false;

        Game.GetPromotion(heroData).Apply(option, healthPerLevel);
        Promoted?.Invoke(heroData);
        return true;
    }

    //Privates
    private bool IsOwned(HeroData heroData) => Game.Deck.Any(hero => hero.Data == heroData);

    //같은 선택지인지 (UI가 이전에 받아 둔 선택지 객체도 허용)
    private static bool IsSameOption(PromotionOption a, PromotionOption b)
        => a.IsEnhance == b.IsEnhance && a.SlotIndex == b.SlotIndex && a.After == b.After;
}
