using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DisPlayUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Sprite[] iconSlots;

    private Image slotImage;
    private PerkItem perk;
    private RectTransform rectTransform;

    public void SetUp(PerkItem perkItem)
    {
        slotImage = GetComponent<Image>();
        int r_int = UnityEngine.Random.Range(0, iconSlots.Length);
        slotImage.sprite = iconSlots[r_int];

        iconImage.sprite = perkItem.Image;
        this.perk = perkItem;
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(perk == null) return;

        TooltipSystem.Instance.Show(rectTransform, perk.Description, perk.Title, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (perk == null) return;

        TooltipSystem.Instance.Hide();
    }
}
