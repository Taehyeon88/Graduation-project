using System;
using System.Linq;
using UnityEngine;

public class RewardSystem : Singleton<RewardSystem>
{
    [Header("딜링 카드")]
    [SerializeField] private CardData[] dealCardDatas;

    [Header("유틸/디버프 카드")]
    [SerializeField] private CardData[] utilCardDatas;

    public Card[] GetRewards(int count)
    {
        if (dealCardDatas.Length + utilCardDatas.Length < count)
            Debug.LogError("보상 카드 종류 개수가 보상UI 필요 카드 개수보다 적음");

        Card[] cards = new Card[count];
        Card card;

        for (int i = 0; i < count; i++)
        {
            float randomValue = UnityEngine.Random.value;
            if (randomValue < 0.7f)
            {
                int randomInt = UnityEngine.Random.Range(0, dealCardDatas.Length);
                card = new(dealCardDatas[randomInt]);
            }
            else
            {
                int randomInt = UnityEngine.Random.Range(0, utilCardDatas.Length);
                card = new(utilCardDatas[randomInt]);
            }

            if (Array.Find(cards, c =>
            {
                if (c != null)
                    if (c.Title == card.Title)
                        return true;
                return false;
            }) != null)
            {
                i--;
                continue;
            }
            else
            {
                cards[i] = card;
            }
        }
        return cards;
    }
}
