using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PileofCardUI : MonoBehaviour
{
    [SerializeField] private RectTransform scrollView;
    [SerializeField] private RectTransform content;
    [SerializeField] private RectTransform cardViewPool;

    [Header("버튼 이벤트")]
    [SerializeField] private Button drawCardPileButton;
    [SerializeField] private Button discardCardPileButton;
    [SerializeField] private Button cancelButton;

    private List<CardViewInPile> cardViewIPs = new();
    private bool isDrawCardPileActive = false;
    private bool isDiscardCardPileActive = false;

    private Transform drawCardPBParent;
    private Transform discardCardPBParent;
    private bool isAbilityActive;
    private void OnEnable()
    {
        drawCardPBParent = drawCardPileButton.transform.parent;
        discardCardPBParent = discardCardPileButton.transform.parent;

        scrollView.gameObject.SetActive(false);

        drawCardPileButton.onClick.AddListener(() => SetDrawPileUI(!isDrawCardPileActive));
        discardCardPileButton.onClick.AddListener(() =>SetDiscardPileUI(!isDiscardCardPileActive));

        cancelButton.onClick.AddListener(() =>
        {
            SetPileofCardUI(isDrawCardPileActive, false);
            isDrawCardPileActive = false;
            isDiscardCardPileActive = false;
        });
    }

    public void SetDrawPileUI(bool active, bool isAbility = false)
    {
        if (active == isDrawCardPileActive 
            || isDiscardCardPileActive
            || (isAbilityActive && !isAbility)) return;

        if (isAbility) isAbilityActive = active;
        isDrawCardPileActive = active;

        SetPileofCardUI(true, active);
        if (active && !isAbility)
            drawCardPileButton.transform.SetParent(scrollView);
    }

    public void SetDiscardPileUI(bool active, bool isAbility = false)
    {
        if (active == isDiscardCardPileActive 
            || isDrawCardPileActive
            || (isAbilityActive && !isAbility)) return;

        if (isAbility) isAbilityActive = active;
        isDiscardCardPileActive = active;

        SetPileofCardUI(false, active);
        if (active && !isAbility)
            discardCardPileButton.transform.SetParent(scrollView);
    }

    public void OffPileofCardUI()
    {
        if (isDiscardCardPileActive) SetDiscardPileUI(false);
        if (isDrawCardPileActive) SetDrawPileUI(false);
    }

    private void SetPileofCardUI(bool isDrawCard, bool active)
    {
        var cards = isDrawCard ? CardSystem.Instance.DrawcardPile : CardSystem.Instance.DiscardPile;
        int cardCount = cards.Count;

        //카드명 사전적 순서로 정렬
        cards.Sort((a,b) => a.Title.CompareTo(b.Title));

        //취소 버튼 설정
        cancelButton.gameObject.SetActive(!isAbilityActive);

        if (active)
        {
            //일시정지 처리
            PauseSystem.Instance.SetPause(true);

            //UI활성화 및 CardPile용 CardView생성
            scrollView.gameObject.SetActive(true);
            int remainCount = cardViewPool.childCount;
            int acturalAmount = Mathf.Min(cardCount, remainCount);
            int createAmount = cardCount - acturalAmount;

            var cardViewsInPile = cardViewPool.GetComponentsInChildren<CardViewInPile>(true);
            for (int i = 0; i < acturalAmount; i++)
            {
                //pool에 남는 것으로 보충
                var card = cards[i];
                var cardViewIP = cardViewsInPile[i];
                cardViewIP.transform.SetParent(content.transform);
                cardViewIP.SetUp(card);
                cardViewIPs.Add(cardViewIP);
            }
            cards.RemoveRange(0, acturalAmount);

            for (int i = 0; i < createAmount; i++)
            {
                //부족한 부분을 생성해서 충당
                var card = cards[i];
                var cardViewIP = CardViewCreator.Instance.CreatCardViewInPile(card, content);
                cardViewIPs.Add(cardViewIP);
            }

            //생성한 카드들의 row에 따라서 Content Top 값 조정
            //row == 1 -> 385 또는 row > 1 -> 181
            var gridLayoutGroup = content.GetComponent<GridLayoutGroup>();
            if (gridLayoutGroup != null)
            {
                bool isOneRow = cardCount <= gridLayoutGroup.constraintCount;
                int value = 181;
                if (isOneRow) value = 385;
                gridLayoutGroup.padding.top = value;
            }

            //content 위치 초기화
            content.anchoredPosition = Vector2.zero;
        }
        else
        {
            //일시정지 취소 처리
            PauseSystem.Instance.SetPause(false);

            //CardPile용 CardView들 전부 pool에 저장
            foreach (var cardViewIP in cardViewIPs)
                cardViewIP.transform.SetParent(cardViewPool);
            cardViewIPs.Clear();

            //버튼 부모 초기화
            if (isDrawCard) drawCardPileButton.transform.SetParent(drawCardPBParent);
            else discardCardPileButton.transform.SetParent(discardCardPBParent);
            //UI 비활성화
            scrollView.gameObject.SetActive(false);
        }
    }
}
