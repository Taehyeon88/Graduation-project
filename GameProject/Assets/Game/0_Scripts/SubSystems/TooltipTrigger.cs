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
        if (rectTransform == null || TooltipSystem.Instance == null) return;   //시작 화면처럼 TooltipSystem이 없는 씬에서는 표시하지 않음 (GAMEMASTER HUD에 붙은 트리거)

        TooltipSystem.Instance.Show(rectTransform, description, title);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (rectTransform == null || TooltipSystem.Instance == null) return;

        TooltipSystem.Instance.Hide();
    }
}
