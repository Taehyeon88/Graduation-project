using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class HandView : MonoBehaviour
{
    [SerializeField] private float spreadAngle = 5f;        //the angle of the spread card
    [SerializeField] private float curveAngle = 3f;         //the angle of the cards curve
    [Range(0f, 1f)]
    [SerializeField] private float spacingRate = 0.35f;      //spacing rate Between the cards (rate * card width)

    private readonly List<CardView> cards = new();
    public IEnumerator AddCard(CardView cardView)
    {
        cards.Add(cardView);
        yield return UpdateCardPositions(0.15f);
    }

    public CardView RemoveCard(Card card)
    {
        CardView cardView = GetCardView(card);
        if (cardView == null) return null;
        cards.Remove(cardView);
        StartCoroutine(UpdateCardPositions(0.15f));
        return cardView;
    }
    private CardView GetCardView(Card card)
    {
        return cards.Find(cardView => cardView.card == card);
    }
    private IEnumerator UpdateCardPositions(float duration)
    {
        if (cards.Count == 0) yield break;

        int cardCount = cards.Count;
        for (int i = 0; i < cardCount; i++)
        {
            RectTransform card = cards[i].GetComponent<RectTransform>();

            float rotateAngle = (i - (cardCount - 1) / 2f) * spreadAngle;
            float angle = (i - (cardCount - 1) / 2f) * curveAngle;
            Vector2 moveDirection = Quaternion.Euler(0, 0, angle) * Vector2.right;
            float moveDistance = (i - (cardCount - 1) / 2f) * spacingRate * card.rect.width;

            card.DOAnchorPos(Vector2.zero - moveDirection * moveDistance, duration);
            card.DORotateQuaternion(Quaternion.Euler(0, 0, rotateAngle), duration);
        }
        yield return new WaitForSeconds(duration);
    }
}
