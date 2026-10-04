using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class HandView : MonoBehaviour
{
    [SerializeField] private float spreadAngle = 5f;        //카드 부채꼴 회전 각도
    [SerializeField] private float curveAngle = 3f;         //카드 배치 곡선 각도
    [Range(0f, 1f)]
    [SerializeField] private float spacingRate = 0.35f;     //카드 간격 비율 (비율 * 카드 너비)
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
