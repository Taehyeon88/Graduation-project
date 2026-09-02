using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private string title;
    [SerializeField] private string description;

    private RectTransform rectTransform;
    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (rectTransform == null) return;

        TooltipSystem.Instance.Show(rectTransform, description, title);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (rectTransform == null) return;

        TooltipSystem.Instance.Hide();
    }
}
