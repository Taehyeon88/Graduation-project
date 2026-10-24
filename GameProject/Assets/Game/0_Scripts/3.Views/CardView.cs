using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Card Card { get; private set; }

    [SerializeField] private Transform manas;   //코스트 슬롯 부모 (코스트 개수만큼 하위 슬롯 활성화)
    [SerializeField] private Image image;

    [Header("Select Direct Element")]
    [SerializeField] private float selectedScale = 1.15f;
    [SerializeField] private float selectDuration = 0.1f;

    private RectTransform rectTransform;
    private bool isHovered = false;

    private void OnDestroy()
    {
        transform.DOKill();
        if (isHovered) TooltipSystem.Instance.Hide();   //호버 중 파괴되면 툴팁 잔존 방지
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
        if (!isHovered) return;

        isHovered = false;
        TooltipSystem.Instance.Hide();
    }

    public void SetSelected(bool selected)
    {
        transform.DOScale(selected ? selectedScale : 1f, selectDuration);
    }

    private void SetCost(int cost)
    {
        for (int i = 0; i < manas.childCount; i++)
            manas.GetChild(i).gameObject.SetActive(i < cost);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (ActionSystem.Instance.IsPerforming
            || Interactions.Instance.IsHeroMoveMode)
            return;

        if (APSystem.Instance.HasEnoughAP(Card.APCost))
        {
            SoundSystem.Instance.PlaySound(3004);        //선택 사운드
            CardSystem.Instance.PlayCardTargetMode(this);
        }
        else
        {
            SoundSystem.Instance.PlaySound(3006);        //코스트 부족 사운드
        }
    }
}
