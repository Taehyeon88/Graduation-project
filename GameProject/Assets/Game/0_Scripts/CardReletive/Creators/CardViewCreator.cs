using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CardViewCreator : Singleton<CardViewCreator>
{
    [SerializeField] private CardView cardViewPrefab;
    [SerializeField] private CardViewInPile cardViewInPilePrefab;

    public CardView CreatCardView(Card card, RectTransform spawnPos, RectTransform parent)
    {
        CardView cardView = Instantiate(cardViewPrefab, spawnPos.position, Quaternion.identity, parent);
        cardView.transform.localScale = Vector3.zero;
        cardView.transform.DOScale(Vector3.one, 0.15f);
        cardView.SetUp(card);
        return cardView;
    }

    public CardViewInPile CreatCardViewInPile(Card card, RectTransform parent)
    {
        CardViewInPile cardViewIP = Instantiate(cardViewInPilePrefab, transform.position, Quaternion.identity, parent);
        cardViewIP.SetUp(card);
        return cardViewIP;
    }
}
