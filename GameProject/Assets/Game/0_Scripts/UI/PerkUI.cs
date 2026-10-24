using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PerkUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image image;

    private Perk perk;
    private RectTransform rectTransform;

    public void Set(Perk perk)
    {
        image.sprite = perk.Image;
        this.perk = perk;
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (perk == null) return;

        TooltipSystem.Instance.Show(rectTransform, perk.Description, perk.Title, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (perk == null) return;

        TooltipSystem.Instance.Hide();
    }
}
