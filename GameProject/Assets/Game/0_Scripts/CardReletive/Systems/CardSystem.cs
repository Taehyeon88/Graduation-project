using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class CardSystem : Singleton<CardSystem>
{
    [SerializeField] private HandView handView;
    [SerializeField] private RectTransform handTransform;
    [SerializeField] private RectTransform drawPilePoint;
    [SerializeField] private RectTransform discardPilePoint;

    private readonly List<Card> drawPile = new();
    private readonly List<Card> discardPile = new();
    private readonly List<Card> hand = new();

    public int drawPileCA => drawPile.Count;    public int discardPileCA => discardPile.Count;

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<DrawCardsGA>(DrawCardsPerformer);
        ActionSystem.AttachPerformer<DiscardAllCardsGA>(DiscardAllCardsPerformer);
        ActionSystem.AttachPerformer<PlayCardGA>(PlayCardPerformer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<DrawCardsGA>();
        ActionSystem.DetachPerformer<DiscardAllCardsGA>();
        ActionSystem.DetachPerformer<PlayCardGA>();
    }

    //Public
    public void SetUp(List<CardData> deckData)
    {
        foreach (var cardData in deckData.Shuffle())
        {
            Card card = new(cardData);
            drawPile.Add(card);
        }
    }
    public void EndLockState()
    {
        handView.SetCardsLockView(false);
        Interactions.Instance.lockInteraction = false;
    }

    //Performers
    private IEnumerator DrawCardsPerformer(DrawCardsGA drawCardsGA)
    {
        Interactions.Instance.lockInteraction = true;  //카드 드로우시, 카드 인터렉션 잠금

        int actualAmount = Mathf.Min(drawCardsGA.Amount, drawPile.Count);
        int notDrawnAmount = drawCardsGA.Amount - actualAmount;
        for (int i = 0; i < actualAmount; i++)
        {
            yield return DrawCard();
        }
        if (notDrawnAmount > 0)
        {
            RefillDect();
            for (int i = 0; i < notDrawnAmount; i++)
                yield return DrawCard();
        }

        if (drawCardsGA.IsFirstDraw) handView.SetCardsLockView(true); //첫턴 카드 드로우시, 바로 카드사용 불가 처리
        else Interactions.Instance.lockInteraction = false;           //드로우 종료후, 잠금 해제
    }
    private IEnumerator DiscardAllCardsPerformer(DiscardAllCardsGA discardAllCardsGA)
    {
        foreach (var card in hand)
        {
            CardView cardView = handView.RemoveCard(card);
            yield return DiscardCard(cardView);
        }
        hand.Clear();
    }
    private IEnumerator PlayCardPerformer(PlayCardGA playCardGA)
    {
        hand.Remove(playCardGA.Card);
        CardView cardView = handView.RemoveCard(playCardGA.Card);
        yield return DiscardCard(cardView);

        SpendManaGA spendManaGA = new(playCardGA.Card.Mana);
        ActionSystem.Instance.AddReaction(spendManaGA);

        if (playCardGA.Card.ManualTargetEffects != null)
        {
            foreach (var effect in playCardGA.Card.ManualTargetEffects)
            {
                PerformEffectGA performEffectGA = new(effect, new(new(){playCardGA.ManualTarget}, HeroSystem.Instance.HeroView));
                ActionSystem.Instance.AddReaction(performEffectGA);
            }
        }

        if (playCardGA.Card.GridTargetMode != null)
        {
            foreach (var targetMode in playCardGA.Card.GridTargetMode)
            {
                foreach (var effect in targetMode.Effects)
                {
                    PerformEffectGA performEffectGA = new(effect, new(targetMode));
                    ActionSystem.Instance.AddReaction(performEffectGA);
                }
            }
        }
    }

    //Helpers
    private IEnumerator DrawCard()
    {
        Card card = drawPile.Draw();
        hand.Add(card);
        CardView cardView = CardViewCreator.Instance.CreatCardView(card, drawPilePoint, handTransform);
        yield return handView.AddCard(cardView);
    }
    private void RefillDect() => drawPile.AddRange(discardPile.Shuffle());

    private IEnumerator DiscardCard(CardView cardView)
    {
        discardPile.Add(cardView.card);
        cardView.transform.DOScale(Vector3.zero, 0.15f);
        Tween tween = cardView.transform.DOMove(discardPilePoint.position, 0.15f);
        yield return tween.WaitForCompletion();
        Destroy(cardView.gameObject);
    }
}
