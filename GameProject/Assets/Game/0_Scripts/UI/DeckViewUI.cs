using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckViewUI : MonoBehaviour   //보유 영웅 전체(GameSystem.Deck)를 카드뷰로 보는 패널
{
    [Header("Deck Panel")]
    [SerializeField] private RectTransform scrollView;
    [SerializeField] private RectTransform content;
    [SerializeField] private RectTransform cardViewPool;

    [Header("Buttons")]
    [SerializeField] private Button deckButton;
    [SerializeField] private Button cancelButton;

    [Header("Count Text")]
    [SerializeField] private TMP_Text countText;

    private readonly List<CardViewInPile> activeViews = new();
    private bool isOpen;

    private void OnEnable()
    {
        deckButton.onClick.AddListener(TogglePanel);
        cancelButton.onClick.AddListener(HidePanel);
    }
    private void OnDisable()
    {
        deckButton.onClick.RemoveListener(TogglePanel);
        cancelButton.onClick.RemoveListener(HidePanel);

        HidePanel();
    }

    private void Start()
    {
        scrollView.gameObject.SetActive(false);
        countText.text = GameSystem.Instance.Deck.Count.ToString();
    }

    //Privates
    private void TogglePanel()
    {
        if (isOpen)
            HidePanel();
        else
            ShowPanel();
    }

    private void ShowPanel()
    {
        ClearViews();

        isOpen = true;
        Interactions.Instance.IsPileViewOpen = true;

        List<Hero> deck = GameSystem.Instance.Deck;
        countText.text = deck.Count.ToString();

        scrollView.gameObject.SetActive(true);

        CardViewInPile[] pooled = cardViewPool.GetComponentsInChildren<CardViewInPile>(true);
        for (int i = 0; i < deck.Count; i++)
        {
            Card card = new(deck[i]);
            CardViewInPile view;
            if (i < pooled.Length)
            {
                view = pooled[i];
                view.transform.SetParent(content, false);
                view.SetUp(card);
            }
            else
            {
                view = CardViewCreator.Instance.CreateCardViewInPile(card, content);
            }
            activeViews.Add(view);
        }

        content.anchoredPosition = Vector2.zero;
    }

    private void HidePanel()
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
}
