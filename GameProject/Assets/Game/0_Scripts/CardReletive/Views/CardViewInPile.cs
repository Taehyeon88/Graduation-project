using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardViewInPile : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text description;
    [SerializeField] private TMP_Text mana;
    [SerializeField] private Image image;
    [SerializeField] private GameObject wrapper;

    private RectTransform rectTransform;
    public Card card { get; private set; }
    public bool lockCardUse { get; set; } = false;

    public void SetUp(Card card, bool isHover = false)
    {
        this.card = card;
        title.text = card.Title;
        description.text = card.Description;
        mana.text = card.Mana.ToString();
        image.sprite = card.Image;
        rectTransform = GetComponent<RectTransform>();
    }
    private void Update()
    {
        //카드 더미의 호버 카드 위치 = 현재 카드 위치 (실시간 업데이트) 
        if (!wrapper.activeSelf)
            CardViewHoverSystem.Instance.ShowInPile(card, rectTransform.position);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        var ev = CardAbilitySystem.Instance.GetCardByDiscardPileEvent;
        if (ev != null)
        {
            ev.Invoke(card);
            OnPointerExit(null);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!Interactions.Instance.PlayerCanHover() || lockCardUse) return;

        //카드 호버 사운드 재생
        SoundSystem.Instance.PlaySound(23);

        wrapper.SetActive(false);
        CardViewHoverSystem.Instance.ShowInPile(card, rectTransform.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!Interactions.Instance.PlayerCanHover() || lockCardUse) return;
        CardViewHoverSystem.Instance.HideInPile();
        wrapper.SetActive(true);
    }
}
