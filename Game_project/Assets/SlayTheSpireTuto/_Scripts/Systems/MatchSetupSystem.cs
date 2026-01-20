using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchSetupSystem : MonoBehaviour
{
    [SerializeField] private List<CardData> deckData;
    private readonly int drawCount = 5;
    private void Start()
    {
        if (deckData == null)
        {
            Debug.LogError("MatchSetUpSystem에 dectData가 설정되지 않았습니다.");
            return;
        }
        if (deckData.Count < drawCount)
        {
            Debug.LogError($"MatchSetUpSystem에 설정된 dectData개수가 {drawCount}개 보다 적습니다.");
            return;
        }

        CardSystem.Instance.SetUp(deckData);
        DrawCardsGA drawCardsGA = new(drawCount);
        ActionSystem.Instance.Perform(drawCardsGA);
    }
}
