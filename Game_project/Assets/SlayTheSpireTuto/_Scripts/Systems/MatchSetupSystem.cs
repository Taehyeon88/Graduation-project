using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchSetupSystem : MonoBehaviour
{
    [SerializeField] private HeroData heroData;
    private readonly int drawCount = 5;
    private void Start()
    {
        if (heroData.Deck == null)
        {
            Debug.LogError("heroData에 deckData가 설정되지 않았습니다.");
            return;
        }
        if (heroData.Deck.Count < drawCount)
        {
            Debug.LogError($"heroData에 설정된 deckData개수가 {drawCount}개 보다 적습니다.");
            return;
        }

        HeroSystem.Instance.SetUp(heroData);
        CardSystem.Instance.SetUp(heroData.Deck);
        DrawCardsGA drawCardsGA = new(drawCount);
        ActionSystem.Instance.Perform(drawCardsGA);
    }
}
