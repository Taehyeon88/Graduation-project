using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//버튼 호버 확대 / 클릭 펀치 (클릭 사운드는 ButtonClickSound)
public class ButtonInteractionUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private const float HoverDuration = 0.15f;
    private const float PunchDuration = 0.2f;

    [SerializeField] private Button button;
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float punchStrength = 0.1f;

    private void OnDisable()
    {
        transform.DOKill();
        transform.localScale = Vector3.one;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!button.interactable) return;
        transform.DOKill();
        transform.DOScale(hoverScale, HoverDuration);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(1f, HoverDuration);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!button.interactable) return;
        transform.DOKill();
        transform.localScale = Vector3.one * hoverScale;
        transform.DOPunchScale(Vector3.one * punchStrength, PunchDuration);
    }
}
