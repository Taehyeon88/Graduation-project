using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class HandView : MonoBehaviour
{
    [Range(0f, 2f)]
    [SerializeField] private float spacingRate = 1.15f;     //카드 간격 비율 (비율 * 카드 너비, 1 초과면 카드 사이에 여유)
    [SerializeField] private float layoutDuration = 0.15f;

    private readonly List<CardView> cards = new();

    //Publics
    public IEnumerator AddCard(CardView cardView)
    {
        cards.Add(cardView);
        yield return UpdateCardPositions(layoutDuration);
    }

    public CardView RemoveCard(Card card)
    {
        CardView cardView = cards.Find(view => view.Card == card);
        if (cardView == null) return null;

        cards.Remove(cardView);
        StartCoroutine(UpdateCardPositions(layoutDuration));
        return cardView;
    }

    //Privates
    private IEnumerator UpdateCardPositions(float duration)
    {
        if (cards.Count == 0) yield break;

        int cardCount = cards.Count;
        for (int i = 0; i < cardCount; i++)
        {
            RectTransform card = cards[i].GetComponent<RectTransform>();

            float moveDistance = (i - (cardCount - 1) / 2f) * spacingRate * card.rect.width;

            card.DOAnchorPos(new Vector2(moveDistance, 0f), duration);
            card.DORotateQuaternion(Quaternion.identity, duration);
        }
        yield return new WaitForSeconds(duration);
    }
}
