using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardViewInPile : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Card Card { get; private set; }

    [SerializeField] private Transform manas;   //코스트 슬롯 부모 (코스트 개수만큼 하위 슬롯 활성화)
    [SerializeField] private Image image;

    private RectTransform rectTransform;
    private bool isHovered = false;

    private void OnDisable()
    {
        HideTooltip();   //더미 패널이 닫히며 풀로 반납(비활성화)될 때 툴팁 잔존 방지
    }

    //Publics
    public void SetUp(Card card)
    {
        Card = card;
        rectTransform = GetComponent<RectTransform>();
        SetCost(card.APCost);
        image.sprite = card.Image;
        image.preserveAspect = true;   //틀은 1:1 고정, 스프라이트 비율 유지
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (Card == null) return;

        isHovered = true;
        TooltipSystem.Instance.Show(rectTransform, Card.Description, Card.Title, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
    }

    //Privates
    private void SetCost(int cost)
    {
        for (int i = 0; i < manas.childCount; i++)
            manas.GetChild(i).gameObject.SetActive(i < cost);
    }

    private void HideTooltip()
    {
        if (!isHovered) return;

        isHovered = false;
        if (TooltipSystem.Instance != null)
            TooltipSystem.Instance.Hide();
    }
}
