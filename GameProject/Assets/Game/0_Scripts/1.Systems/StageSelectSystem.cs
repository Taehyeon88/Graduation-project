using System.Collections.Generic;
using DG.Tweening;
using IsoTools;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class StageSelectSystem : Singleton<StageSelectSystem>
{
    [SerializeField] private StructureData[] stageStructures;   //스테이지 번호 순서대로 나열한 임시 1자형 리스트 (랜덤 생성 전)
    [SerializeField] private StructureView structurePrefab;
    [SerializeField] private IsoWorld isoWorld;                 //IsoObject 위치가 적용되려면 IsoWorld 하위에 생성해야 함
    [SerializeField] private Vector3 firstPosition;             //첫 구조물의 Iso 위치
    [SerializeField] private Vector3 spacing = new Vector3(2f, 0f, 0f);   //구조물 간 Iso 간격

    [SerializeField] private StageHeroView hero;                //스테이지 선택 연출용 영웅
    [SerializeField] private RectTransform tooltipAnchor;       //구조물 툴팁 위치 기준점 (호버 시 StructureView가 구조물 화면 좌표로 옮김)

    private readonly Dictionary<int, StructureView> structureCache = new();   //스테이지 번호(1부터) → 구조물
    private bool isMoving;                                      //영웅 이동·Fade 중 입력 잠금
    private StructureView hoveredStructure;                     //현재 마우스가 올라간 구조물

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;
    }

    private void Start()
    {
        if (Instance != this) return;

        CreateStructures();
        RefreshSelectable();

        //직전 스테이지 위치에서 시작 (첫 스테이지는 씬 배치 위치)
        if (structureCache.TryGetValue(GameSystem.Instance.CurrentStageLevel - 1, out StructureView previous))
        {
            hero.SetPosition(previous.IsoPosition);
        }

        if (structureCache.Count >= 2)
        {
            StageCameraSystem.Instance.SetGridRange(structureCache[1].WorldCenter, structureCache[structureCache.Count].WorldCenter);
        }
        StageCameraSystem.Instance.FocusOnStart(previous, GetSelectableStructure());
    }

    private void Update()
    {
        if (isMoving)
        {
            UpdateHover(null);
            return;
        }

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        bool isClick = StageCameraSystem.Instance.UpdateDrag();
        UpdateHover(StageCameraSystem.Instance.IsDragging ? null : FindMouseOverStructure(mouseWorld));

        if (!isClick) return;

        StructureView selectable = GetSelectableStructure();
        if (selectable != null && selectable.IsMouseOver(mouseWorld))
        {
            isMoving = true;
            GameSystem.Instance.SelectStructure(selectable.Data);
            StageCameraSystem.Instance.FocusOn(selectable, false);
            hero.PlayMove(selectable.IsoPosition)
                .OnComplete(() => FadeSystem.Instance.LoadSceneWithFade(selectable.Data.SceneName));
        }
    }

    //잠긴 구조물도 정보 확인은 가능해야 하므로 선택 가능 여부와 무관하게 탐색
    private StructureView FindMouseOverStructure(Vector3 mouseWorld)
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return null;

        foreach (StructureView view in structureCache.Values)
        {
            if (view.IsMouseOver(mouseWorld)) return view;
        }
        return null;
    }

    private void UpdateHover(StructureView target)
    {
        if (target == hoveredStructure) return;

        if (hoveredStructure != null) hoveredStructure.OnHoverExit();
        hoveredStructure = target;
        if (hoveredStructure != null) hoveredStructure.OnHoverEnter(tooltipAnchor);
    }

    private void CreateStructures()
    {
        for (int i = 0; i < stageStructures.Length; i++)
        {
            int stageNumber = i + 1;
            StructureView view = Instantiate(structurePrefab, isoWorld.transform);
            view.SetUp(stageStructures[i], stageNumber, firstPosition + spacing * i);
            structureCache.Add(stageNumber, view);
        }
    }

    //지금 플레이할 스테이지(CurrentStageLevel)만 선택 가능
    private void RefreshSelectable()
    {
        StructureView selectable = GetSelectableStructure();
        foreach (StructureView view in structureCache.Values)
        {
            view.SetSelectable(view == selectable);
        }
    }

    private StructureView GetSelectableStructure()
    {
        structureCache.TryGetValue(GameSystem.Instance.CurrentStageLevel, out StructureView view);
        return view;
    }
}
