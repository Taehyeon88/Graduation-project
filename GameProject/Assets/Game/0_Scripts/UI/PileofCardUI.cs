using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PileofCardUI : MonoBehaviour
{
    [Header("Pile Panel")]
    [SerializeField] private RectTransform scrollView;
    [SerializeField] private RectTransform content;
    [SerializeField] private RectTransform cardViewPool;

    [Header("Buttons")]
    [SerializeField] private Button drawPileButton;
    [SerializeField] private Button discardPileButton;
    [SerializeField] private Button cancelButton;

    [Header("Count Texts")]
    [SerializeField] private TMP_Text drawPileCountText;
    [SerializeField] private TMP_Text discardPileCountText;

    private readonly List<CardViewInPile> activeViews = new();
    private bool isOpen;
    private bool isDrawPileOpen;

    private void OnEnable()
    {
        ActionSystem.SubscribeReaction<DrawCardsGA>(PilesChangedReaction, ReactionTiming.POST);
        ActionSystem.SubscribeReaction<PlayCardGA>(PilesChangedReaction, ReactionTiming.POST);
        CardSystem.PilesReset += RefreshCount;

        drawPileButton.onClick.AddListener(OnDrawPileButton);
        discardPileButton.onClick.AddListener(OnDiscardPileButton);
        cancelButton.onClick.AddListener(HidePile);
    }
    private void OnDisable()
    {
        ActionSystem.UnsubscribeReaction<DrawCardsGA>(PilesChangedReaction, ReactionTiming.POST);
        ActionSystem.UnsubscribeReaction<PlayCardGA>(PilesChangedReaction, ReactionTiming.POST);
        CardSystem.PilesReset -= RefreshCount;

        drawPileButton.onClick.RemoveListener(OnDrawPileButton);
        discardPileButton.onClick.RemoveListener(OnDiscardPileButton);
        cancelButton.onClick.RemoveListener(HidePile);

        HidePile();
    }

    private void Start()
    {
        scrollView.gameObject.SetActive(false);
        RefreshCount();
    }

    //Privates
    private void OnDrawPileButton() => TogglePile(true);
    private void OnDiscardPileButton() => TogglePile(false);

    private void TogglePile(bool isDrawPile)
    {
        if (isOpen && isDrawPileOpen == isDrawPile)
            HidePile();
        else
            ShowPile(isDrawPile);
    }

    private void ShowPile(bool isDrawPile)
    {
        ClearViews();

        isOpen = true;
        isDrawPileOpen = isDrawPile;
        Interactions.Instance.IsPileViewOpen = true;

        List<Card> cards = isDrawPile ? CardSystem.Instance.DrawPileCards : CardSystem.Instance.DiscardPileCards;
        if (isDrawPile)
            cards.Sort((a, b) => string.Compare(a.Title, b.Title));   //뽑는 순서 노출 방지

        scrollView.gameObject.SetActive(true);

        CardViewInPile[] pooled = cardViewPool.GetComponentsInChildren<CardViewInPile>(true);
        for (int i = 0; i < cards.Count; i++)
        {
            CardViewInPile view;
            if (i < pooled.Length)
            {
                view = pooled[i];
                view.transform.SetParent(content, false);
                view.SetUp(cards[i]);
            }
            else
            {
                view = CardViewCreator.Instance.CreateCardViewInPile(cards[i], content);
            }
            activeViews.Add(view);
        }

        content.anchoredPosition = Vector2.zero;
    }

    private void HidePile()
    {
        if (!isOpen) return;

        ClearViews();
        scrollView.gameObject.SetActive(false);

        isOpen = false;
        if (Interactions.Instance != null)
            Interactions.Instance.IsPileViewOpen = false;
    }

    //CardView들을 전부 pool에 반납
    private void ClearViews()
    {
        foreach (var view in activeViews)
            view.transform.SetParent(cardViewPool, false);
        activeViews.Clear();
    }

    private void RefreshCount()
    {
        drawPileCountText.text = CardSystem.Instance.DrawPileCount.ToString();
        discardPileCountText.text = CardSystem.Instance.DiscardPileCount.ToString();
    }

    //Reactions
    private void PilesChangedReaction(GameAction gameAction)
    {
        RefreshCount();
    }
}
