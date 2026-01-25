using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardViewHoverSystem : Singleton<CardViewHoverSystem>
{
    [SerializeField] private CardView cardViewHover;

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
}
