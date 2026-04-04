using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardAbilitySystem : Singleton<CardAbilitySystem>
{
    public readonly List<CardAbilityType> Abilitys = new();
    public Func<float> AddNextAdjDamageEvent { get; private set; }             //다음 인접 카드 50% 증가
    public Action<Card> GetCardByDiscardPileEvent { get; private set; }        //버려진 카드 더미에서 카드 가져오기

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<AddCardAbilityGA>(AddCardAbilityGAPerformer);
        ActionSystem.SubscribeReaction<EnemysTurnGA>(EnemysTurnPreReaction, ReactionTiming.PRE);
        ActionSystem.SubscribeReaction<PlayCardGA>(PlayCardPreReaction, ReactionTiming.PRE);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<AddCardAbilityGA>();
        ActionSystem.UnsubscribeReaction<EnemysTurnGA>(EnemysTurnPreReaction, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<PlayCardGA>(PlayCardPreReaction, ReactionTiming.PRE);
    }

    //Performers
    private IEnumerator AddCardAbilityGAPerformer(AddCardAbilityGA addCardAbilityGA)
    {
        Debug.Log("카드능력 추가 - " +  addCardAbilityGA.CardAbilityType);
        Abilitys.Add(addCardAbilityGA.CardAbilityType);


        //버려진 카드 더미에서 카드 가져오기
        if (Abilitys.Contains(CardAbilityType.GetCardByDiscardPile))
        {
            bool eventInvoke = false;
            UISystem.Instance.SetPileofCardUI(false, true, true);
            GetCardByDiscardPileEvent = (card) =>
            {
                UISystem.Instance.SetPileofCardUI(false, false, true);
                eventInvoke = true;
                Abilitys.Remove(CardAbilityType.GetCardByDiscardPile);

                DrawCardFromDiscardPileGA drawCardFDPGA = new(card);
                ActionSystem.Instance.AddReaction(drawCardFDPGA);
            };

            yield return new WaitUntil(() =>  eventInvoke);
        }

        yield return null;
    }


    //Subscribers
    private void EnemysTurnPreReaction(EnemysTurnGA enemysTurnGA)
    {
        if (Abilitys.Count <= 0) return;

        //다음 인접 카드 50% 증가 - 턴 종료시 삭제 처리
        if (Abilitys.Contains(CardAbilityType.AddNextAdjDamage))
            Abilitys.Remove(CardAbilityType.AddNextAdjDamage);
    }

    private void PlayCardPreReaction(PlayCardGA playCardGA)
    {
        //다음 인접 카드 50% 증가 - 효과 처리
        if (Abilitys.Contains(CardAbilityType.AddNextAdjDamage))
        {
            if (playCardGA.Card.CardType == CardType.Attck_Adjacent)
            {
                Effect effect = playCardGA.Card.GridTargetMode.Effect;
                if (effect != null)
                {
                    //Debug.Log("카드 효과 걸기");
                    AddNextAdjDamageEvent = () =>
                    {
                        //Debug.Log("카드 효과 적용");
                        return 0.5f;
                    };
                    Abilitys.Remove(CardAbilityType.AddNextAdjDamage);
                }
            }
        }
    }
}
