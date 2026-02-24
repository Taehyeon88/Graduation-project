using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroSystem : Singleton<HeroSystem>
{
    public HeroView HeroView => TokenSystem.Instance.HeroView;

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<HeroFirstMoveGA>(HeroFirstMoveGAPerformer);
        ActionSystem.SubscribeReaction<EnemysTurnGA>(EnemysTurnPreReaction, ReactionTiming.PRE);
        ActionSystem.SubscribeReaction<EnemysTurnGA>(EnemysTurnPostReaction, ReactionTiming.POST);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<HeroFirstMoveGA>();
        ActionSystem.UnsubscribeReaction<EnemysTurnGA>(EnemysTurnPreReaction, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<EnemysTurnGA>(EnemysTurnPostReaction, ReactionTiming.POST);
    }

    //Performers
    private IEnumerator HeroFirstMoveGAPerformer(HeroFirstMoveGA heroFristMoveGA)
    {
        //플레이어 이동 가능 여부 판단
        var canMovePlaces = TokenSystem.Instance.GetCanMovePlace(HeroView, heroFristMoveGA.SPD);
        if (canMovePlaces == null || canMovePlaces.Count == 0)
        {
            //false -> 플레이어 피격 및 액션 모드 전환
            int amount = Mathf.CeilToInt(HeroView.MaxHealth * 0.05f);
            DealDamageGA dealDamageGA = new(amount, new() { HeroView }, HeroView);
            ActionSystem.Instance.AddReaction(dealDamageGA);

            ControllModeSystem.Instance.ChangeControllMode(ControllMode.Action);
        }
        else
        {
            //true -> 플레이어 이동 모드 -> 이동후, 액션모드전환
            SPDSystem.Instance.AddSPD(heroFristMoveGA.SPD);

            ControllModeSystem.Instance.ChangeControllMode(ControllMode.Move);
        }

        yield return null;
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

        HeroFirstMoveGA heroFristMove = new(1);
        ActionSystem.Instance.AddReaction(heroFristMove);
    }
}
