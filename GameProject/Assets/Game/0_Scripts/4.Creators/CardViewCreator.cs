using DG.Tweening;
using UnityEngine;

public class CardViewCreator : Singleton<CardViewCreator>
{
    [SerializeField] private CardView cardViewPrefab;
    [SerializeField] private CardViewInPile cardViewInPilePrefab;
    [SerializeField] private float spawnTweenDuration = 0.15f;

    public CardView CreateCardView(Card card, RectTransform spawnPos, RectTransform parent)
    {
        CardView cardView = Instantiate(cardViewPrefab, spawnPos.position, Quaternion.identity, parent);
        cardView.transform.SetAsFirstSibling();
        cardView.transform.localScale = Vector3.zero;
        cardView.transform.DOScale(Vector3.one, spawnTweenDuration);
        cardView.SetUp(card);
        return cardView;
    }

    public CardViewInPile CreateCardViewInPile(Card card, RectTransform parent)
    {
        CardViewInPile cardViewInPile = Instantiate(cardViewInPilePrefab, parent);
        cardViewInPile.SetUp(card);
        return cardViewInPile;
    }
}
