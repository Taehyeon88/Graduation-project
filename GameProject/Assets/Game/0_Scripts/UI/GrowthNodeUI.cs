using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//스테이지 선택 씬의 공용 트리 화면: 본부 중심 노드 그물(한 화면) · 노드 클릭 = 1레벨 구매 · 호버 = 효과·선행 조건 · 전체 초기화
//노드 슬롯은 씬에 직접 배치, 연결선은 열 때 슬롯 위치로 생성 (규칙 판단·실행은 GrowthNodeSystem)
public class GrowthNodeUI : MonoBehaviour
{
    [SerializeField] private Button openButton;   //화면 열기 (항상 표시)
    [SerializeField] private GameObject panel;
    [SerializeField] private Button closeButton;
    [SerializeField] private TMP_Text starsText;
    [SerializeField] private Button resetButton;

    [Header("트리")]
    [SerializeField] private GrowthNodeSlotUI[] slots;        //씬에 배치한 노드 슬롯 (같은 부모 안)
    [SerializeField] private RectTransform headquarters;      //본부 (선행 조건 없는 노드의 연결선 시작점)
    [SerializeField] private RectTransform lineContent;       //연결선 부모 (슬롯과 같은 좌표계, 슬롯보다 뒤)
    [SerializeField] private Image lineTemplate;
    [SerializeField] private float lineWidth = 4f;
    [SerializeField] private Color lineSatisfiedColor = new(0.85f, 0.75f, 0.35f, 1f);
    [SerializeField] private Color lineUnsatisfiedColor = new(0.35f, 0.35f, 0.4f, 1f);

    [Header("노드 색")]
    [SerializeField] private Color captainColor = new(0.6f, 0.25f, 0.25f, 1f);
    [SerializeField] private Color resourceColor = new(0.6f, 0.5f, 0.2f, 1f);
    [SerializeField] private Color heroColor = new(0.25f, 0.4f, 0.65f, 1f);
    [SerializeField] private Color tacticsColor = new(0.25f, 0.55f, 0.3f, 1f);
    [SerializeField] private Color mixedColor = new(0.45f, 0.3f, 0.6f, 1f);
    [SerializeField] private Color lockedColor = new(0.22f, 0.22f, 0.26f, 1f);
    [SerializeField] private Color notImplementedColor = new(0.15f, 0.15f, 0.17f, 1f);
    [SerializeField] private float notEnoughStarsDim = 0.6f;   //별 부족일 때 가지 색 밝기

    [Header("노드 정보 (호버)")]
    [SerializeField] private TMP_Text infoText;

    [Header("초기화 확인")]
    [SerializeField] private GameObject confirmPanel;
    [SerializeField] private TMP_Text confirmText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button confirmCancelButton;

    public bool IsOpen => panel.activeSelf;

    private readonly List<(Image line, GrowthNodePrerequisite? prerequisite)> lines = new();   //prerequisite가 없으면 본부 연결
    private GrowthNodeSlotUI hoveredSlot;

    private GrowthNodeSystem Growth => GrowthNodeSystem.Instance;

    private void Awake()
    {
        panel.SetActive(false);
        confirmPanel.SetActive(false);
        lineTemplate.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        openButton.onClick.AddListener(Open);
        closeButton.onClick.AddListener(Close);
        resetButton.onClick.AddListener(OpenConfirm);
        confirmButton.onClick.AddListener(ConfirmReset);
        confirmCancelButton.onClick.AddListener(CloseConfirm);
        foreach (GrowthNodeSlotUI slot in slots)
        {
            slot.Clicked += OnSlotClicked;
            slot.HoverEntered += OnSlotHoverEntered;
            slot.HoverExited += OnSlotHoverExited;
        }
    }

    private void OnDisable()
    {
        openButton.onClick.RemoveListener(Open);
        closeButton.onClick.RemoveListener(Close);
        resetButton.onClick.RemoveListener(OpenConfirm);
        confirmButton.onClick.RemoveListener(ConfirmReset);
        confirmCancelButton.onClick.RemoveListener(CloseConfirm);
        foreach (GrowthNodeSlotUI slot in slots)
        {
            slot.Clicked -= OnSlotClicked;
            slot.HoverEntered -= OnSlotHoverEntered;
            slot.HoverExited -= OnSlotHoverExited;
        }
        if (IsOpen) Growth.Changed -= Refresh;
    }

    //Publics
    public void Open()
    {
        if (IsOpen) return;

        Growth.Changed += Refresh;
        panel.SetActive(true);
        if (lines.Count == 0) CreateLines();
        hoveredSlot = null;
        Refresh();
    }

    public void Close()
    {
        if (!IsOpen) return;

        Growth.Changed -= Refresh;
        CloseConfirm();
        panel.SetActive(false);
    }

    //Privates
    private void Refresh()
    {
        starsText.SetText($"남은 별 {Growth.AvailableStars}  /  획득 {GameSystem.Instance.Stars}");
        resetButton.interactable = Growth.SpentStars > 0;

        foreach (GrowthNodeSlotUI slot in slots)
        {
            GrowthNodeState state = Growth.GetState(slot.Node);
            slot.SetView(Growth.GetLevel(slot.Node), GetStateSuffix(state), GetStateColor(slot.Node, state));
        }

        foreach (var (line, prerequisite) in lines)
            line.color = prerequisite == null || Growth.IsSatisfied(prerequisite.Value) ? lineSatisfiedColor : lineUnsatisfiedColor;

        RefreshInfo();
    }

    private void RefreshInfo()
    {
        if (hoveredSlot == null)
        {
            infoText.SetText("노드에 마우스를 올리면 효과와 선행 조건이 보입니다.\n\n노드를 클릭하면 별 1개로 1레벨 올립니다.");
            return;
        }

        GrowthNodeData node = hoveredSlot.Node;
        int level = Growth.GetLevel(node);
        GrowthNodeState state = Growth.GetState(node);

        StringBuilder sb = new();
        sb.AppendLine($"<size=120%>{node.Code} {node.Title}</size>");
        sb.AppendLine($"레벨 {level} / {node.MaxLevel}");
        sb.AppendLine();
        sb.AppendLine($"현재: {GrowthNodeSystem.GetEffectText(node, level) ?? "없음"}");
        if (level < node.MaxLevel) sb.AppendLine($"다음: {GrowthNodeSystem.GetEffectText(node, level + 1)}");
        sb.AppendLine();
        sb.AppendLine("선행 조건");
        if (node.Prerequisites.Count == 0) sb.AppendLine("  본부 (없음)");
        foreach (GrowthNodePrerequisite prerequisite in node.Prerequisites)
        {
            string mark = Growth.IsSatisfied(prerequisite) ? "<color=#8f8>[충족]</color>" : "<color=#f88>[미충족]</color>";   //폰트에 체크 기호가 없을 수 있어 글자로 표시
            sb.AppendLine($"  {mark} {prerequisite.Node.Code} {prerequisite.Node.Title} {prerequisite.Level}+ (현재 {Growth.GetLevel(prerequisite.Node)})");
        }
        sb.AppendLine();
        sb.Append(GetStateMessage(state));
        infoText.SetText(sb.ToString());
    }

    private void OnSlotClicked(GrowthNodeSlotUI slot)
    {
        Growth.Purchase(slot.Node);   //성공하면 Changed로 갱신
    }

    private void OnSlotHoverEntered(GrowthNodeSlotUI slot)
    {
        hoveredSlot = slot;
        if (IsOpen) RefreshInfo();
    }

    private void OnSlotHoverExited(GrowthNodeSlotUI slot)
    {
        if (hoveredSlot != slot) return;
        hoveredSlot = null;
        if (IsOpen) RefreshInfo();
    }

    private void OpenConfirm()
    {
        if (Growth.SpentStars == 0) return;

        confirmText.SetText($"공용 트리를 초기화합니다.\n\n쓴 별 {Growth.SpentStars}개를 모두 돌려받고\n모든 노드가 레벨 0이 됩니다.");
        confirmPanel.SetActive(true);
    }

    private void CloseConfirm()
    {
        confirmPanel.SetActive(false);
    }

    private void ConfirmReset()
    {
        CloseConfirm();
        Growth.ResetAll();   //Changed로 갱신
    }

    //선행 노드(없으면 본부) → 노드 연결선. 슬롯 위치가 고정이라 처음 열 때 한 번만 생성
    private void CreateLines()
    {
        Dictionary<GrowthNodeData, RectTransform> positions = slots.ToDictionary(slot => slot.Node, slot => slot.RectTransform);
        foreach (GrowthNodeSlotUI slot in slots)
        {
            if (slot.Node.Prerequisites.Count == 0)
            {
                lines.Add((CreateLine(headquarters.anchoredPosition, slot.RectTransform.anchoredPosition), null));
                continue;
            }

            foreach (GrowthNodePrerequisite prerequisite in slot.Node.Prerequisites)
            {
                if (!positions.TryGetValue(prerequisite.Node, out RectTransform from)) continue;
                lines.Add((CreateLine(from.anchoredPosition, slot.RectTransform.anchoredPosition), prerequisite));
            }
        }
    }

    private Image CreateLine(Vector2 from, Vector2 to)
    {
        Image line = Instantiate(lineTemplate, lineContent);
        line.gameObject.SetActive(true);
        RectTransform rect = line.rectTransform;
        Vector2 delta = to - from;
        rect.anchoredPosition = (from + to) * 0.5f;
        rect.sizeDelta = new Vector2(delta.magnitude, lineWidth);
        rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
        return line;
    }

    private Color GetStateColor(GrowthNodeData node, GrowthNodeState state)
    {
        Color branchColor = GetBranchColor(node.Branch);
        return state switch
        {
            GrowthNodeState.NOT_IMPLEMENTED => notImplementedColor,
            GrowthNodeState.LOCKED => lockedColor,
            GrowthNodeState.NOT_ENOUGH_STARS => new Color(branchColor.r * notEnoughStarsDim, branchColor.g * notEnoughStarsDim, branchColor.b * notEnoughStarsDim, 1f),
            _ => branchColor,   //구매 가능·최대
        };
    }

    private Color GetBranchColor(GrowthBranchType branch) => branch switch
    {
        GrowthBranchType.CAPTAIN => captainColor,
        GrowthBranchType.RESOURCE => resourceColor,
        GrowthBranchType.HERO => heroColor,
        GrowthBranchType.TACTICS => tacticsColor,
        _ => mixedColor,
    };

    private static string GetStateSuffix(GrowthNodeState state) => state switch
    {
        GrowthNodeState.NOT_IMPLEMENTED => "  준비 중",
        GrowthNodeState.LOCKED => "  잠김",
        GrowthNodeState.MAX => "  최대",
        _ => "",
    };

    private static string GetStateMessage(GrowthNodeState state) => state switch
    {
        GrowthNodeState.NOT_IMPLEMENTED => "<color=#aaa>효과 준비 중 — 아직 구매할 수 없습니다</color>",
        GrowthNodeState.LOCKED => "<color=#f88>선행 조건을 모두 채우면 열립니다</color>",
        GrowthNodeState.MAX => "<color=#fd8>최대 레벨입니다</color>",
        GrowthNodeState.NOT_ENOUGH_STARS => "<color=#f88>별이 부족합니다 (1레벨 = 별 1)</color>",
        _ => "<color=#8f8>클릭하면 별 1개로 1레벨 올립니다</color>",
    };
}
