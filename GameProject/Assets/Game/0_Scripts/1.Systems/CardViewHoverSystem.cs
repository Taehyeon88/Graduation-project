using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardViewHoverSystem : Singleton<CardViewHoverSystem>
{
    [SerializeField] private CardView cardViewHover;
    [SerializeField] private CardViewInPile cardViewHoverInPile;
    [SerializeField] private CardViewInPile cardViewHoverInReward;

    private bool isLocked = false;
    public void Show(Card card, Vector2 position, bool isLock = false)
    {
        cardViewHover.gameObject.SetActive(true);
        cardViewHover.transform.SetAsLastSibling();

        cardViewHover.SetUp(card, true);
        cardViewHover.GetComponent<RectTransform>().anchoredPosition = position;
        if (isLock)
        {
            isLocked = isLock;
            SetCardsLockView(isLock, cardViewHover);
        }
    }
    public void Hide()
    {
        cardViewHover.gameObject.SetActive(false);
        if (isLocked)
        {
            isLocked = !isLocked;
            SetCardsLockView(isLocked, cardViewHover);
        }
    }

    public void ShowInPile(Card card, Vector2 position)
    {
        cardViewHoverInPile.gameObject.SetActive(true);
        cardViewHoverInPile.SetUp(card, true);
        cardViewHoverInPile.GetComponent<RectTransform>().position = position;
    }
    public void HideInPile()
    {
        cardViewHoverInPile.gameObject.SetActive(false);
    }

    public void ShowInReward(Card card, Vector2 position)
    {
        cardViewHoverInReward.gameObject.SetActive(true);
        cardViewHoverInReward.SetUp(card, true);
        cardViewHoverInReward.GetComponent<RectTransform>().position = position;
    }
    public void HideInReward()
    {
        cardViewHoverInReward.gameObject.SetActive(false);
    }


    private void SetCardsLockView(bool active, CardView cardViewHover)
    {
        if (active)
        {
            foreach (var image in cardViewHover.Images)
                if (image != null) image.color = new Color32(100, 100, 100, 255);
        }
        else
        {
            foreach (var image in cardViewHover.Images)
                if (image != null) image.color = Color.white;
        }
    }
}
