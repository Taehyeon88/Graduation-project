using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheckDeckUI : MonoBehaviour
{
    [SerializeField] private RectTransform scrollView;
    [SerializeField] private RectTransform content;
    [SerializeField] private RectTransform cardViewPool;

    private List<CardViewInPile> cardViewIPs = new();

    public void SetCheckDeckUI(bool active)
    {
        var cards = new List<CardData>(GameSystem.Instance.Deck);
        int cardCount = cards.Count;

        if (active)
        {
            //카드더미 활성화 사운드 재생
            SoundSystem.Instance.PlaySound(30);

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
                cardViewIP.SetUp(new(card));
                cardViewIPs.Add(cardViewIP);
            }
            cards.RemoveRange(0, acturalAmount);

            for (int i = 0; i < createAmount; i++)
            {
                //부족한 부분을 생성해서 충당
                var card = cards[i];
                var cardViewIP = CardViewCreator.Instance.CreatCardViewInPile(new(card), content);
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
            //카드더미 비 활성화 사운드 재생
            SoundSystem.Instance.PlaySound(31);

            //일시정지 취소 처리
            PauseSystem.Instance.SetPause(false);

            //CardPile용 CardView들 전부 pool에 저장
            foreach (var cardViewIP in cardViewIPs)
                cardViewIP.transform.SetParent(cardViewPool);
            cardViewIPs.Clear();

            //UI 비활성화
            scrollView.gameObject.SetActive(false);
        }
    }
}
