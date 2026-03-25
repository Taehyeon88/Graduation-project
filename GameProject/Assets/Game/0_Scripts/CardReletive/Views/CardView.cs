using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardView : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler
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
        if (!Interactions.Instance.PlayerCanInteract() || lockCardUse) return;

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
        if (!Interactions.Instance.PlayerCanInteract() || lockCardUse) return;
        transform.position = eventData.position;

        //그리드 미리보기
        if (card.SelfEffects != null && card.SelfEffects.Count > 0)
        {
            Vector2Int heroPos = TokenSystem.Instance.GetTokenPosition(HeroSystem.Instance.HeroView);
            VisualGridCreator.Instance.CreateVisualGrid(gameObject.GetInstanceID(), heroPos, "Hero_UseSelf");
        }

        if (card.GridTargetMode != null)
        {
            var targetMode = card.GridTargetMode;

            Vector2Int currentPos = TokenSystem.Instance.GetTokenPosition(HeroSystem.Instance.HeroView);
            bool penetration = targetMode.TargetMode is LineTM;
            var range = targetMode.GridRangeMode.GetGridRanges(currentPos, targetMode.Distance, penetration);

            //이동 카드일 경우, 자체적인 이동 범위 사용
            if (targetMode.Effect is PlayerMoveEffect)
            {
                range = TokenSystem.Instance.GetCanMovePlace(HeroSystem.Instance.HeroView, targetMode.Distance);
            }

            //비주얼 공격 예상 범위 그리드 업데이트
            foreach (var r in range)
                VisualGridCreator.Instance.CreateVisualGrid(gameObject.GetInstanceID(), r, targetMode.WillSelectVGName);
        }
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!Interactions.Instance.PlayerCanInteract() || lockCardUse) return;

        if (ManaSystem.Instance.HasEnoughMana(card.Mana) && rectTransform.anchoredPosition.y > 250f) //카드가 y좌표 250를 넘었음 = DropArea
        {
            PlayCardGA playCardGA = new(card);
            ActionSystem.Instance.Perform(playCardGA);
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
