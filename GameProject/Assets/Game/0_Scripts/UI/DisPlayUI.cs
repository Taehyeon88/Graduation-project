using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DisPlayUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Sprite[] iconSlots;

    private Image slotImage;
    private Trait trait;
    private RectTransform rectTransform;

    public void SetUp(Trait trait)
    {
        slotImage = GetComponent<Image>();
        int r_int = UnityEngine.Random.Range(0, iconSlots.Length);
        slotImage.sprite = iconSlots[r_int];

        iconImage.sprite = trait.Image;
        this.trait = trait;
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(trait == null) return;

        TooltipSystem.Instance.Show(rectTransform, trait.Description, trait.Title, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (trait == null) return;

        TooltipSystem.Instance.Hide();
    }
}
