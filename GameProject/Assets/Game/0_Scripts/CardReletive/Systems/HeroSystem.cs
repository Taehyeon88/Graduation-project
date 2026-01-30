using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroSystem : Singleton<HeroSystem>
{
    public HeroView HeroView => TokenSystem.Instance.HeroView;

    private void OnEnable()
    {
        ActionSystem.SubscribeReaction<EnemyTurnGA>(EnemyTurnPreReaction, ReactionTiming.PRE);
        ActionSystem.SubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);
        ActionSystem.SubscribeReaction<RollDiceGA>(RollDicePostReaction, ReactionTiming.POST);
    }
    private void OnDisable()
    {
        ActionSystem.UnsubscribeReaction<EnemyTurnGA>(EnemyTurnPreReaction, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);
        ActionSystem.UnsubscribeReaction<RollDiceGA>(RollDicePostReaction, ReactionTiming.POST);
    }

    //Reactions
    private void EnemyTurnPreReaction(EnemyTurnGA enemyTurnGA)
    {
        DiscardAllCardsGA discardAllCardsGA = new();
        ActionSystem.Instance.AddReaction(discardAllCardsGA);
    }
    private void EnemyTurnPostReaction(EnemyTurnGA enemyTurnGA)
    {
        int burnStacks = HeroView.GetStatusEffectStacks(StatusEffectType.BURN);
        if (burnStacks > 0)
        {
            ApplyBurnGA applyBurnGA = new(burnStacks, HeroView);
            ActionSystem.Instance.AddReaction(applyBurnGA);
        }
    }

    private void RollDicePostReaction(RollDiceGA rollDiceGA)
    {
        DrawCardsGA drawCardsGA = new DrawCardsGA(5);
        ActionSystem.Instance.AddReaction(drawCardsGA);
    }
}
