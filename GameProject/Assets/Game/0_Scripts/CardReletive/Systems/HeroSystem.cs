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
        bool isFirsMove = true;

        //상태 이상 - 고립 처리
        int isolationStack = HeroView.GetStatusEffectStacks(StatusEffectType.ISOLATION);
        if (isolationStack > 0) 
            isFirsMove = false;

        //플레이어 카드 드로우
        DrawCardsGA drawCardGA = new(5, isFirsMove);
        ActionSystem.Instance.AddReaction(drawCardGA);

        if (!isFirsMove)
        {
            yield break;
        }

        //플레이어 이동 가능 여부 판단
        var canMovePlaces = TokenSystem.Instance.GetCanMovePlace(HeroView, heroFristMoveGA.SPD);
        if (canMovePlaces == null || canMovePlaces.Count == 0)
        {
            //false -> 플레이어 피격 및 카드사용 가능
            int amount = Mathf.CeilToInt(HeroView.MaxHealth * 0.05f);
            DealDamageGA dealDamageGA = new(amount, new() { HeroView }, HeroView);
            ActionSystem.Instance.AddReaction(dealDamageGA, FinishedFirstMoveRelated);
        }
        else
        {
            //true -> 플레이어 이동 모드 -> 이동 및 카드사용 가능
            SPDSystem.Instance.AddSPD(heroFristMoveGA.SPD);
            PlayerMoveGA playerMoveGA = new(heroFristMoveGA.SPD, true);
            ActionSystem.Instance.AddReaction(playerMoveGA, FinishedFirstMoveRelated);
        }

        yield return null;
    }


    //Reactions
    private void EnemysTurnPreReaction(EnemysTurnGA enemyTurnGA)
    {
        Debug.Log("플레이어 턴 종료");

        DiscardAllCardsGA discardAllCardsGA = new();
        ActionSystem.Instance.AddReaction(discardAllCardsGA);

        //플레이어 상태효과 N감소
        foreach (var statusEffectType in HeroView.GetStatusEffects())
        {
            //방어막은 제외
            if (statusEffectType != StatusEffectType.ARMOR)
            {
                HeroView.RemoveStatusEffect(statusEffectType, 1);
            }
        }
    }
    private void EnemysTurnPostReaction(EnemysTurnGA enemyTurnGA)
    {
        Debug.Log("플레이어 턴 시작");

        //플레이어의 방어막 스택 삭제
        int armorStack = HeroView.GetStatusEffectStacks(StatusEffectType.ARMOR);
        if (armorStack > 0) HeroView.RemoveStatusEffect(StatusEffectType.ARMOR, armorStack);

        //상태 효과
        //악화
        float specialRate = 1;
        int deteriorateStacks = HeroView.GetStatusEffectStacks(StatusEffectType.DETERIORATE);
        if (deteriorateStacks > 0)
        {
            float rate = HeroView.GetStatusEffectInfo(StatusEffectType.DETERIORATE).Deteriorate_Rate;
            specialRate = specialRate * rate;
        }

        //독물
        int poisionStatcks = HeroView.GetStatusEffectStacks(StatusEffectType.POISIONING);
        if (poisionStatcks > 0)
        {
            float percent = HeroView.GetStatusEffectInfo(StatusEffectType.POISIONING).Poision_Percent;
            int amount = Mathf.CeilToInt(HeroView.MaxHealth * (percent / 100f) * specialRate);
            DealDamageGA dealDamageGA = new(amount, new() { HeroView }, HeroView, DamageFormulaType.Special);
            ActionSystem.Instance.AddReaction(dealDamageGA);
        }

        //출혈
        int bleedingStatcks = HeroView.GetStatusEffectStacks(StatusEffectType.BLEEDING);
        if (bleedingStatcks > 0)
        {
            float percent = HeroView.GetStatusEffectInfo(StatusEffectType.BLEEDING).Bleeding_Percent;
            int amount = Mathf.CeilToInt(HeroView.MaxHealth * (percent / 100f) * specialRate);
            DealDamageGA dealDamageGA = new(amount, new() { HeroView }, HeroView, DamageFormulaType.Special);
            ActionSystem.Instance.AddReaction(dealDamageGA);
        }


        //화염 상태효과 
        int burnStacks = HeroView.GetStatusEffectStacks(StatusEffectType.BURN);
        if (burnStacks > 0)
        {
            ApplyBurnGA applyBurnGA = new(burnStacks, HeroView);
            ActionSystem.Instance.AddReaction(applyBurnGA);
        }

        HeroFirstMoveGA heroFristMove = new(1);
        ActionSystem.Instance.AddReaction(heroFristMove);
    }

    //Finisheds
    private void FinishedFirstMoveRelated() => CardSystem.Instance.EndLockState();
}
