using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroSystem : Singleton<HeroSystem>
{
    public HeroView HeroView => TokenSystem.Instance.HeroView;

    private void OnEnable()
    {
        ActionSystem.SubscribeReaction<EnemysTurnGA>(EnemysTurnPreReaction, ReactionTiming.PRE);
        ActionSystem.SubscribeReaction<EnemysTurnGA>(EnemysTurnPostReaction, ReactionTiming.POST);
        ActionSystem.SubscribeReaction<RollDiceGA>(RollDicePostReaction, ReactionTiming.POST);
    }
    private void OnDisable()
    {
        ActionSystem.UnsubscribeReaction<EnemysTurnGA>(EnemysTurnPreReaction, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<EnemysTurnGA>(EnemysTurnPostReaction, ReactionTiming.POST);
        ActionSystem.UnsubscribeReaction<RollDiceGA>(RollDicePostReaction, ReactionTiming.POST);
    }

    //Reactions
    private void EnemysTurnPreReaction(EnemysTurnGA enemyTurnGA)
    {
        Debug.Log("플레이어 턴 종료");

        DiscardAllCardsGA discardAllCardsGA = new();
        ActionSystem.Instance.AddReaction(discardAllCardsGA);
    }
    private void EnemysTurnPostReaction(EnemysTurnGA enemyTurnGA)
    {
        Debug.Log("적 턴 종료");

        int burnStacks = HeroView.GetStatusEffectStacks(StatusEffectType.BURN);
        if (burnStacks > 0)
        {
            ApplyBurnGA applyBurnGA = new(burnStacks, HeroView);
            ActionSystem.Instance.AddReaction(applyBurnGA);
        }
        RollDiceGA rollDiceGA = new();
        ActionSystem.Instance.AddReaction(rollDiceGA);
    }

    private void RollDicePostReaction(RollDiceGA rollDiceGA)
    {
        DrawCardsGA drawCardsGA = new DrawCardsGA(5);
        ActionSystem.Instance.AddReaction(drawCardsGA);
    }
}
