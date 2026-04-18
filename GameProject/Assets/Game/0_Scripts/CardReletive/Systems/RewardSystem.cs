using UnityEngine;

public class RewardSystem : Singleton<RewardSystem>
{
    [Header("딜링 카드")]
    [SerializeField] private CardData[] dealCardDatas;

    [Header("유틸/디버프 카드")]
    [SerializeField] private CardData[] utilCardDatas;

    public Card[] GetRewards(int count)
    {
        Card[] card = new Card[count];

        for (int i = 0; i < count; i++)
        {
            float randomValue = Random.value;
            if (randomValue < 0.7f)
            {
                int randomInt = Random.Range(0, dealCardDatas.Length);
                card[i] = new(dealCardDatas[randomInt]);
            }
            else
            {
                int randomInt = Random.Range(0, utilCardDatas.Length);
                card[i] = new(utilCardDatas[randomInt]);
            }
        }
        return card;
    }
}
