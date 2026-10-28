using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;

//스테이지 선택 씬 카메라 담당: 구조물 포커스 이동 + 그리드 방향 드래그 이동
public class StageCameraSystem : Singleton<StageCameraSystem>
{
    [SerializeField] private Transform cameraFocus;             //Cinemachine이 Follow하는 대상
    [SerializeField] private CinemachineCamera virtualCamera;   //instant 포커스 시 Damping 초기화용
    [SerializeField] private float dragMargin = 4f;             //첫/마지막 구조물 밖으로 더 갈 수 있는 거리 (그리드 축 기준, 월드 단위)
    [SerializeField] private float dragThreshold = 8f;          //클릭과 드래그를 가르는 픽셀 거리

    public bool IsDragging { get; private set; }

    private bool hasGrid;                                       //그리드 축이 설정돼야 드래그 이동 가능
    private Vector2 gridOrigin;                                 //첫 구조물 중심
    private Vector2 gridAxis;                                   //그리드가 이어지는 방향 (정규화)
    private float gridLength;                                   //첫~마지막 구조물 거리

    private bool pressActive;                                   //누른 채 아직 안 뗌
    private Vector2 pressScreenPos;
    private Vector2 lastScreenPos;

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;
    }

    //Publics
    //첫~마지막 구조물 중심으로 드래그 축·범위를 정함 (호출하지 않으면 드래그 이동 비활성)
    public void SetGridRange(Vector3 firstCenter, Vector3 lastCenter)
    {
        Vector2 delta = lastCenter - firstCenter;
        gridLength = delta.magnitude;
        if (gridLength <= 0f) return;

        gridOrigin = firstCenter;
        gridAxis = delta / gridLength;
        hasGrid = true;
    }

    public void FocusOnStart(StructureView previous, StructureView current)
    {
        StartCoroutine(FocusOnStartRoutine(previous, current));
    }

    public void FocusOn(StructureView view, bool instant)
    {
        Vector3 center = view.WorldCenter;
        cameraFocus.position = new Vector3(center.x, center.y, cameraFocus.position.z);
        if (instant) virtualCamera.PreviousStateIsValid = false;
    }

    //클릭(드래그 없이 누름→뗌)이 완료된 프레임에만 true. 드래그 중이면 그리드 축으로만 카메라 이동
    public bool UpdateDrag()
    {
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUI())
        {
            pressActive = true;
            pressScreenPos = lastScreenPos = Input.mousePosition;
        }

        if (!pressActive) return false;

        if (Input.GetMouseButton(0))
        {
            Vector2 screenPos = Input.mousePosition;
            if (!IsDragging && (screenPos - pressScreenPos).magnitude >= dragThreshold) IsDragging = true;
            if (IsDragging) DragFocus(screenPos - lastScreenPos);
            lastScreenPos = screenPos;
            return false;
        }

        bool isClick = !IsDragging;
        pressActive = false;
        IsDragging = false;
        return isClick;
    }

    //Privates
    //직전 구조물에 즉시 맞춘 뒤 한 프레임 뒤 현재 구조물로 이동 (같은 프레임이면 Damping 이동이 보이지 않음)
    private IEnumerator FocusOnStartRoutine(StructureView previous, StructureView current)
    {
        if (current == null) yield break;

        if (previous == null)
        {
            FocusOn(current, true);
            yield break;
        }

        FocusOn(previous, true);
        yield return null;
        FocusOn(current, false);
    }

    private void DragFocus(Vector2 screenDelta)
    {
        if (!hasGrid) return;

        //ScreenToWorldPoint 차이는 카메라가 움직이는 중 흔들리므로 픽셀→월드 비율로 변환
        Vector2 worldDelta = -screenDelta * (2f * Camera.main.orthographicSize / Screen.height);
        float t = Vector2.Dot((Vector2)cameraFocus.position - gridOrigin, gridAxis) + Vector2.Dot(worldDelta, gridAxis);
        t = Mathf.Clamp(t, -dragMargin, gridLength + dragMargin);

        Vector2 position = gridOrigin + gridAxis * t;
        cameraFocus.position = new Vector3(position.x, position.y, cameraFocus.position.z);
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
    }
}
