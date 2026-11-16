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
    [SerializeField] private StageInfoPopupUI stageInfoPopup;   //전투 구조물 클릭 시 여는 정보 팝업 (영웅 카드 선택 → 진입)

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

        //마지막으로 들어간 스테이지 위치에서 시작 (아직 들어간 적 없으면 씬 배치 위치)
        StructureView previous = null;
        if (GameSystem.Instance.CurrentStructure != null)
        {
            structureCache.TryGetValue(GameSystem.Instance.CurrentStageLevel, out previous);
            if (previous != null) hero.SetPosition(previous.IsoPosition);
        }

        if (structureCache.Count >= 2)
        {
            StageCameraSystem.Instance.SetGridRange(structureCache[1].WorldCenter, structureCache[structureCache.Count].WorldCenter);
        }
        StageCameraSystem.Instance.FocusOnStart(previous, GetFrontierStructure());
    }

    private void Update()
    {
        if (isMoving || stageInfoPopup.IsOpen)   //이동 중이거나 정보 팝업이 열려 있으면 월드 입력 잠금
        {
            UpdateHover(null);
            return;
        }

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        bool isClick = StageCameraSystem.Instance.UpdateDrag();
        UpdateHover(StageCameraSystem.Instance.IsDragging ? null : FindMouseOverStructure(mouseWorld));

        if (!isClick) return;

        StructureView selectable = hoveredStructure;
        if (selectable == null || !IsUnlocked(selectable.StageNumber)) return;

        //전투 구조물은 정보 팝업에서 영웅 카드를 고른 뒤 진입, 비전투 구조물(주점 등)은 바로 진입
        if (selectable.Data.StageData != null)
            stageInfoPopup.Open(selectable.Data, selection => EnterStructure(selectable, selection));
        else
            EnterStructure(selectable, new List<Hero>());
    }

    private void EnterStructure(StructureView structure, List<Hero> selection)
    {
        isMoving = true;
        GameSystem.Instance.EnterStage(structure.Data, structure.StageNumber, selection);
        StageCameraSystem.Instance.FocusOn(structure, false);
        hero.PlayMove(structure.IsoPosition)
            .OnComplete(() => FadeSystem.Instance.LoadSceneWithFade(structure.Data.SceneName));
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

    //해금된 스테이지(첫 스테이지 또는 이전 스테이지 클리어)는 모두 선택 가능 — 클리어한 스테이지 재도전 포함
    private void RefreshSelectable()
    {
        foreach (StructureView view in structureCache.Values)
        {
            view.SetState(IsUnlocked(view.StageNumber), GameSystem.Instance.GetProgress(view.Data));
        }
    }

    private bool IsUnlocked(int stageNumber)
    {
        if (stageNumber <= 1) return true;
        return structureCache.TryGetValue(stageNumber - 1, out StructureView previous)
            && GameSystem.Instance.GetProgress(previous.Data).IsCleared;
    }

    //해금됐지만 아직 클리어하지 않은 첫 스테이지 (모두 클리어했으면 마지막 스테이지) — 카메라 포커스 대상
    private StructureView GetFrontierStructure()
    {
        for (int number = 1; number <= structureCache.Count; number++)
        {
            StructureView view = structureCache[number];
            if (IsUnlocked(number) && !GameSystem.Instance.GetProgress(view.Data).IsCleared) return view;
        }
        return structureCache.Count > 0 ? structureCache[structureCache.Count] : null;
    }
}
