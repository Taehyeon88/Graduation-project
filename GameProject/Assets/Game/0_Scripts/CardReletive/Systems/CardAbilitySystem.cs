using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardAbilitySystem : Singleton<CardAbilitySystem>
{
    public readonly List<CardAbilityType> Abilitys = new();

    private Dictionary<CardAbilityType, Func<float>> EventByAbilityType = new();

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
                    Debug.Log("카드 효과 걸기");
                    EventByAbilityType.Add(CardAbilityType.AddNextAdjDamage, () =>
                    {
                        Debug.Log("카드 효과 적용");
                        EventByAbilityType.Remove(CardAbilityType.AddNextAdjDamage);
                        return 0.5f;
                    });
                    Abilitys.Remove(CardAbilityType.AddNextAdjDamage);
                }
            }
        }
    }

    //Publics
    public Func<float> GetCardAbilityEvent(CardAbilityType cardAbilityType)
    {
        if (EventByAbilityType.ContainsKey(cardAbilityType))
        {
            return EventByAbilityType[cardAbilityType];
        }
        return null;
    }
}
