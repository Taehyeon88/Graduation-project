using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardViewHoverSystem : Singleton<CardViewHoverSystem>
{
    [SerializeField] private CardView cardViewHover;
    [SerializeField] private CardViewInPile cardViewHoverInPile;

    public void Show(Card card, Vector2 position)
    {
        cardViewHover.gameObject.SetActive(true);
        cardViewHover.SetUp(card);
        cardViewHover.GetComponent<RectTransform>().anchoredPosition = position;
    }
    public void Hide()
    {
        cardViewHover.gameObject.SetActive(false);
    }

    public void ShowInPile(Card card, Vector2 position)
    {
        cardViewHoverInPile.gameObject.SetActive(true);
        cardViewHoverInPile.SetUp(card);
        cardViewHoverInPile.GetComponent<RectTransform>().position = position;
    }
    public void HideInPile()
    {
        cardViewHoverInPile.gameObject.SetActive(false);
    }
}
