using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PerkUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image image;
    [SerializeField] private GameObject countRoot;   //횟수 표시 묶음(배경 + 텍스트), 전투당 횟수 제한이 없는 유물이면 통째로 숨김
    [SerializeField] private TMP_Text countText;   //전투당 횟수 제한이 있는 유물만 남은 횟수 표시 (우하단)
    [SerializeField] private Color exhaustedColor = new(0.4f, 0.4f, 0.4f, 1f);   //횟수를 모두 쓴 아이콘 색

    private Perk perk;
    private RectTransform rectTransform;

    private void OnDestroy()
    {
        if (perk != null) perk.OnCountChanged -= Refresh;
    }

    public void Set(Perk perk)
    {
        image.sprite = perk.Image;
        this.perk = perk;
        rectTransform = GetComponent<RectTransform>();
        perk.OnCountChanged += Refresh;
        Refresh();
    }

    private void Refresh()
    {
        if (countRoot != null) countRoot.SetActive(perk.HasLimit);
        if (countText != null && perk.HasLimit) countText.SetText(perk.RemainingTriggers.ToString());
        image.color = perk.IsExhausted ? exhaustedColor : Color.white;
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
