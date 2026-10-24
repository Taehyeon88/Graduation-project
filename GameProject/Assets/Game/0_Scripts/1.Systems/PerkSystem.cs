using System.Collections.Generic;
using UnityEngine;

public class PerkSystem : Singleton<PerkSystem>
{
    [SerializeField] private PerksUI perksUI;

    private readonly List<Perk> perks = new();   //유저가 소유한 유물 (전투 씬마다 GameSystem.Perks로 다시 구성)

    private void OnEnable()
    {
        ActionSystem.SubscribeReaction<TurnGA>(TurnGAPreReaction, ReactionTiming.PRE);
    }
    private void OnDisable()
    {
        ActionSystem.UnsubscribeReaction<TurnGA>(TurnGAPreReaction, ReactionTiming.PRE);
        foreach (Perk perk in perks)
            perk.OnRemove();
        perks.Clear();
    }

    public void SetUp()
    {
        foreach (PerkData perkData in GameSystem.Instance.Perks)
        {
            Perk perk = new(perkData);
            perk.OnAdd();
            perks.Add(perk);
        }
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
