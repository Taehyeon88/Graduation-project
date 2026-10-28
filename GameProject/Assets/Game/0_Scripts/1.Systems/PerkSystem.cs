using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PerkSystem : Singleton<PerkSystem>
{
    [SerializeField] private PerksUI perksUI;

    private readonly List<Perk> perks = new();   //유저가 소유한 유물 (GameSystem.Perks로 씬마다 다시 구성, 전투에서만 구독)

    private void OnEnable()
    {
        ActionSystem.SubscribeReaction<TurnGA>(TurnGAPreReaction, ReactionTiming.PRE);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable()
    {
        ActionSystem.UnsubscribeReaction<TurnGA>(TurnGAPreReaction, ReactionTiming.PRE);
        SceneManager.sceneLoaded -= OnSceneLoaded;
        ClearPerks();
    }

    //전투 시작: 유물 효과 구독까지 켠다
    public void SetUp()
    {
        ClearPerks();
        foreach (PerkData perkData in GameSystem.Instance.Perks)
        {
            Perk perk = new(perkData);
            perk.OnAdd();
            perks.Add(perk);
        }
        perksUI.SetUp(perks);
    }

    private void ClearPerks()
    {
        foreach (Perk perk in perks)
            perk.OnRemove();
        perks.Clear();
    }

    //Events
    //씬이 바뀌면 이전 전투의 구독을 풀고, 보유 유물을 구독 없이 표시만 한다 (전투 씬은 이후 SetUp이 덮어씀)
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (cannotInitialize) return;   //Singleton이 파괴할 씬 쪽 복제본

        ClearPerks();
        foreach (PerkData perkData in GameSystem.Instance.Perks)
            perks.Add(new Perk(perkData));
        perksUI.SetUp(perks);
    }

    //Reactions
    private void TurnGAPreReaction(TurnGA turnGA)
    {
        if (turnGA.Type != TurnType.StartBattle) return;

        foreach (Perk perk in perks)
            perk.ResetBattleCount();
    }
}
