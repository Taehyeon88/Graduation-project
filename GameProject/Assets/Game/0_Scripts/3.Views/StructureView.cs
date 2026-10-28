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

    public StructureData Data { get; private set; }
    public int StageNumber { get; private set; }   //스테이지 번호 (캐싱 키)
    public Vector3 IsoPosition => isoObject.position;   //노드의 Iso 위치 (영웅 이동 목표)
    public Vector3 WorldCenter => model.bounds.center;   //스프라이트 월드 중심 (카메라 포커스 목표)

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
        SetSelectable(false);
    }

    public void SetSelectable(bool selectable)
    {
        model.color = selectable ? selectableColor : lockedColor;
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
