using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CardViewCreator : Singleton<CardViewCreator>
{
    [SerializeField] private CardView cardViewPrefab;

    public CardView CreatCardView(Card card, RectTransform spawnPos, RectTransform parent)
    {
        CardView cardView = Instantiate(cardViewPrefab, spawnPos.position, Quaternion.identity, parent);
        cardView.transform.localScale = Vector3.zero;
        cardView.transform.DOScale(Vector3.one, 0.15f);
        cardView.SetUp(card);
        return cardView;
    }
}
