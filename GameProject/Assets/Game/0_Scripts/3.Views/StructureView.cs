using IsoTools;
using UnityEngine;

public class StructureView : MonoBehaviour
{
    private const int HoverSoundId = 3001;   //전투씬 호버 대상 진입음(GridSelector)과 동일

    [SerializeField] private SpriteRenderer model;
    [SerializeField] private IsoObject isoObject;
    [SerializeField] private SpriteRenderer highlight;   //호버 시 표시하는 흰색 테두리
    [SerializeField] private Color selectableColor = Color.white;
    [SerializeField] private Color lockedColor = new Color(0.4f, 0.4f, 0.4f, 1f);

    [Header("상태 표시 (잠김 / 선택 가능 / 클리어)")]
    [SerializeField] private SpriteRenderer lockIcon;      //잠김일 때만 표시
    [SerializeField] private SpriteRenderer[] starIcons;   //별 3칸 — 해금된 전투 구조물만 표시 (비전투 구조물은 숨김)
    [SerializeField] private Sprite filledStarSprite;
    [SerializeField] private Sprite emptyStarSprite;

    public StructureData Data { get; private set; }
    public int StageNumber { get; private set; }   //스테이지 번호 (캐싱 키)
    public Vector3 IsoPosition => isoObject.position;   //노드의 Iso 위치 (영웅 이동 목표)
    public Vector3 WorldCenter => model.bounds.center;   //스프라이트 월드 중심 (카메라 포커스 목표)
    public Bounds ModelBounds => model.bounds;           //스프라이트 월드 영역 (튜토리얼 스포트라이트 구멍용)

    private bool isHovered;

    private void OnDisable()
    {
        OnHoverExit();   //호버 중 비활성화되면 툴팁 잔존 방지
    }

    public void SetUp(StructureData data, int stageNumber, Vector3 isoPosition)
    {
        Data = data;
        StageNumber = stageNumber;
        model.sprite = data.Sprite;
        isoObject.position = isoPosition;
        SetState(false, null);
    }

    //잠김: 어둡게 + 자물쇠 / 선택 가능: 빈 별 3개 / 클리어: 획득한 별만 채움
    //replayLocked: 해금됐지만 다시 들어갈 수 없음 (클리어한 튜토리얼 스테이지) — 별은 보이되 어둡게 + 자물쇠
    public void SetState(bool unlocked, StageProgress progress, bool replayLocked = false)
    {
        bool selectable = unlocked && !replayLocked;
        model.color = selectable ? selectableColor : lockedColor;
        lockIcon.enabled = !selectable;

        bool showStars = unlocked && Data.StageData != null;
        for (int i = 0; i < starIcons.Length; i++)
        {
            starIcons[i].enabled = showStars;
            starIcons[i].sprite = progress != null && progress.HasStar(i) ? filledStarSprite : emptyStarSprite;
        }
    }

    public bool IsMouseOver(Vector3 mouseWorld)
    {
        Bounds bounds = model.bounds;
        mouseWorld.z = bounds.center.z;
        return bounds.Contains(mouseWorld);
    }

    public void OnHoverEnter(RectTransform anchor)
    {
        if (isHovered) return;
        isHovered = true;
        highlight.enabled = true;
        SoundSystem.Instance.PlaySound(HoverSoundId);
        anchor.position = Camera.main.WorldToScreenPoint(WorldCenter);   //구조물 위치 기준으로 SetMode가 pivot·간격을 정함
        TooltipSystem.Instance.Show(anchor, Data.Description, Data.name);
    }

    public void OnHoverExit()
    {
        if (!isHovered) return;
        isHovered = false;
        highlight.enabled = false;
        TooltipSystem.Instance.Hide();
    }
}
