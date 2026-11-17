using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//공용 트리 노드 1개 (씬에 가지별 위치로 배치) — 표시만 하고 클릭·호버는 GrowthNodeUI에 알림
public class GrowthNodeSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GrowthNodeData node;
    [SerializeField] private Button button;
    [SerializeField] private Image background;
    [SerializeField] private TMP_Text label;   //코드·이름·레벨(2/3)

    public GrowthNodeData Node => node;
    public RectTransform RectTransform => (RectTransform)transform;

    public event System.Action<GrowthNodeSlotUI> Clicked;
    public event System.Action<GrowthNodeSlotUI> HoverEntered;
    public event System.Action<GrowthNodeSlotUI> HoverExited;

    private void OnEnable()
    {
        button.onClick.AddListener(OnClick);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(OnClick);
    }

    //Publics
    public void SetView(int level, string stateText, Color color)
    {
        background.color = color;
        label.SetText($"{node.Code} {node.Title}\n<size=80%>{level}/{node.MaxLevel}{stateText}</size>");
    }

    public void OnPointerEnter(PointerEventData eventData) => HoverEntered?.Invoke(this);

    public void OnPointerExit(PointerEventData eventData) => HoverExited?.Invoke(this);

    //Privates
    private void OnClick() => Clicked?.Invoke(this);
}
