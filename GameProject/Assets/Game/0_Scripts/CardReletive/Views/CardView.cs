using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardView : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text description;
    [SerializeField] private TMP_Text mana;
    [SerializeField] private Image image;
    [SerializeField] private GameObject wrapper;

    private RectTransform rectTransform;
    private Vector3 dragStartPosition;
    private Quaternion dragStartRotation;
    public Card card { get; private set; }
    public bool lockCardUse { get; set; } = false;

    public void SetUp(Card card)
    {
        this.card = card;
        title.text = card.Title;
        description.text = card.Description;
        mana.text = card.Mana.ToString();
        image.sprite = card.Image;
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!Interactions.Instance.PlayerCanTargeting() || lockCardUse) return;

        if (ManaSystem.Instance.HasEnoughMana(card.Mana))
        {
            Interactions.Instance.PlayerIsTargeting = true;

            //플레이어 타겟팅 모드 진입
            wrapper.SetActive(false);
            Vector2 pos = rectTransform.anchoredPosition + Vector2.up * 125f;
            CardViewHoverSystem.Instance.Show(card, pos);

            Action endSelectGrid = () =>
            {
                Interactions.Instance.PlayerIsTargeting = false;
                CardViewHoverSystem.Instance.Hide();
                if (wrapper != null)
                    wrapper.SetActive(true);
            };

            PlayCardTargetingGA playCardTargetingGA = new(card, endSelectGrid);
            ActionSystem.Instance.Perform(playCardTargetingGA, endSelectGrid);
        }
        else
        {
            //코스트 부족으로 사용불가 연출
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!Interactions.Instance.PlayerCanHover() || lockCardUse) return;
        wrapper.SetActive(false);
        Vector2 pos = rectTransform.anchoredPosition + Vector2.up * 120f;
        CardViewHoverSystem.Instance.Show(card, pos);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!Interactions.Instance.PlayerCanHover() || lockCardUse) return;
        CardViewHoverSystem.Instance.Hide();
        wrapper.SetActive(true);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!Interactions.Instance.PlayerCanDraging() || lockCardUse) return;

        Interactions.Instance.PlayerIsDraging = true;
        wrapper.SetActive(true);
        CardViewHoverSystem.Instance.Hide();
        dragStartPosition = transform.position;
        dragStartRotation = transform.rotation;
        transform.rotation = Quaternion.Euler(0, 0, 0);
        transform.position = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!Interactions.Instance.PlayerCanDraging() || lockCardUse) return;

        transform.position = eventData.position;

        //그리드 미리보기
        if (card.SelfEffects != null && card.SelfEffects.Count > 0 && card.GridTargetMode.GridRangeMode == null)
        {
            Vector2Int heroPos = TokenSystem.Instance.GetTokenPosition(HeroSystem.Instance.HeroView);
            VisualGridCreator.Instance.CreateVisualGrid(gameObject.GetInstanceID(), heroPos, "Hero_UseSelf");
        }
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!Interactions.Instance.PlayerCanDraging() || lockCardUse) return;

        if (!Interactions.Instance.PlayerIsDraging) return;

        if (ManaSystem.Instance.HasEnoughMana(card.Mana) && rectTransform.anchoredPosition.y > 250f) //카드가 y좌표 250를 넘었음 = DropArea
        {
            if (card.GridTargetMode.GridRangeMode != null)
            {
                transform.position = dragStartPosition;
                transform.rotation = dragStartRotation;

                //플레이어 타겟팅 모드 진입
                Interactions.Instance.PlayerIsTargeting = true;
                wrapper.SetActive(false);
                Vector2 pos = rectTransform.anchoredPosition + Vector2.up * 125f;
                CardViewHoverSystem.Instance.Show(card, pos);

                Action endSelectGrid = () =>
                {
                    Interactions.Instance.PlayerIsTargeting = false;
                    CardViewHoverSystem.Instance.Hide();
                    if (wrapper != null)
                        wrapper.SetActive(true);
                };

                PlayCardTargetingGA playCardTargetingGA = new(card, endSelectGrid);
                ActionSystem.Instance.Perform(playCardTargetingGA, endSelectGrid);
            }
            else
            {
                PlayCardGA playCardGA = new(card, null);
                ActionSystem.Instance.Perform(playCardGA);
            }
        }
        else
        {
            transform.position = dragStartPosition;
            transform.rotation = dragStartRotation;
        }

        //미리보기 취소
        VisualGridCreator.Instance.RemoveVisualGridById(gameObject.GetInstanceID());
        Interactions.Instance.PlayerIsDraging = false;
    }
}
