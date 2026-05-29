using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardView : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public Card Card { get; private set; }
    public Image[] Images { get; private set; }
    public bool LockCardUse { get; set; } = false;

    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text description;
    [SerializeField] private TMP_Text mana;
    [SerializeField] private Image image;
    [SerializeField] private GameObject wrapper;

    private RectTransform rectTransform;
    private Vector3 dragStartPosition;
    private Quaternion dragStartRotation;

    private GraphicRaycaster raycaster;

    public void SetUp(Card card, bool isHover = false)
    {
        this.Card = card;
        title.text = card.Title;
        description.text = card.Description;
        mana.text = card.Mana.ToString();
        image.sprite = card.Image;
        rectTransform = GetComponent<RectTransform>();

        if (isHover)
        {
            Images = GetComponentsInChildren<Image>(true);
        }
        else
        {
            Images = GetComponentsInChildren<Image>(true);
            Images[0] = null;
        }

        raycaster = GetComponentInParent<GraphicRaycaster>();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!Interactions.Instance.PlayerCanTargeting() || LockCardUse) return;

        if (ManaSystem.Instance.HasEnoughMana(Card.Mana))
        {
            //카드 선택 사운드 재생
            SoundSystem.Instance.PlaySound(18);

            Interactions.Instance.PlayerIsTargeting = true;

            //플레이어 타겟팅 모드 진입
            wrapper.SetActive(false);
            Vector2 pos = rectTransform.anchoredPosition + Vector2.up * 125f;
            CardViewHoverSystem.Instance.Show(Card, pos);

            Action endSelectGrid = () =>
            {
                Interactions.Instance.PlayerIsTargeting = false;
                CardViewHoverSystem.Instance.Hide();
                if (wrapper != null)
                    wrapper.SetActive(true);
            };

            PlayCardTargetingGA playCardTargetingGA = new(Card, endSelectGrid);
            ActionSystem.Instance.Perform(playCardTargetingGA, endSelectGrid);
        }
        else
        {
            //코스트 부족 사운드 재생
            SoundSystem.Instance.PlaySound(21);

            //코스트 부족으로 사용불가 연출
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!Interactions.Instance.PlayerCanHover()) return;

        //카드 호버 사운드 재생
        SoundSystem.Instance.PlaySound(17);

        wrapper.SetActive(false);
        Vector2 pos = rectTransform.anchoredPosition + Vector2.up * 120f;
        CardViewHoverSystem.Instance.Show(Card, pos, 
                        Interactions.Instance.lockInteraction || LockCardUse);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!Interactions.Instance.PlayerCanHover()) return;
        CardViewHoverSystem.Instance.Hide();
        wrapper.SetActive(true);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!Interactions.Instance.PlayerCanDraging() || LockCardUse) return;

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
        if (!Interactions.Instance.PlayerCanDraging() || LockCardUse) return;

        transform.position = eventData.position;

        //그리드 미리보기
        if (Card.SelfEffects != null && Card.SelfEffects.Count > 0 && Card.GridTargetMode.GridRangeMode == null)
        {
            Vector2Int heroPos = TokenSystem.Instance.GetTokenPosition(HeroSystem.Instance.HeroView);
            VisualGridCreator.Instance.CreateVisualGrid(gameObject.GetInstanceID(), heroPos, "Hero_UseSelf");
        }

        //다른 UI 감지
        bool isDetecting = false;
        List<RaycastResult> results = new List<RaycastResult>();

        raycaster.Raycast(eventData, results);
        foreach (RaycastResult result in results)
        {
            if (result.gameObject.CompareTag("MOVP"))
                isDetecting = true;
        }

        if (isDetecting && !Interactions.Instance.IsMovpDetecting)
        {
            //감지됨 처리

            Interactions.Instance.IsMovpDetecting = true;
        }
        else if (!isDetecting && Interactions.Instance.IsMovpDetecting)
        {
            //감지됨 취소 처리

            Interactions.Instance.IsMovpDetecting = false;
        }
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!Interactions.Instance.PlayerCanDraging() || LockCardUse) return;

        if (!Interactions.Instance.PlayerIsDraging) return;

        if (Interactions.Instance.IsMovpDetecting)
        {
            //해당 카드 버리기 및 이동 포인트 증가
            AddSpdGA addSpdGA = new AddSpdGA();
            ActionSystem.Instance.Perform(addSpdGA);

            DiscardCardGA discardCardGA = new DiscardCardGA(this);
            ActionSystem.Instance.AddReaction(discardCardGA);

            return;
        }

        if (ManaSystem.Instance.HasEnoughMana(Card.Mana) && rectTransform.anchoredPosition.y > 250f) //카드가 y좌표 250를 넘었음 = DropArea
        {
            if (Card.GridTargetMode.GridRangeMode != null)
            {
                transform.position = dragStartPosition;
                transform.rotation = dragStartRotation;

                //플레이어 타겟팅 모드 진입
                Interactions.Instance.PlayerIsTargeting = true;
                wrapper.SetActive(false);
                Vector2 pos = rectTransform.anchoredPosition + Vector2.up * 125f;
                CardViewHoverSystem.Instance.Show(Card, pos);

                Action endSelectGrid = () =>
                {
                    Interactions.Instance.PlayerIsTargeting = false;
                    CardViewHoverSystem.Instance.Hide();
                    if (wrapper != null)
                        wrapper.SetActive(true);
                };

                PlayCardTargetingGA playCardTargetingGA = new(Card, endSelectGrid);
                ActionSystem.Instance.Perform(playCardTargetingGA, endSelectGrid);
            }
            else
            {
                PlayCardGA playCardGA = new(Card, null);
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
