using System.Collections.Generic;
using IsoTools;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageSelectSystem : Singleton<StageSelectSystem>
{
    [SerializeField] private StructureData[] stageStructures;   //스테이지 번호 순서대로 나열한 임시 1자형 리스트 (랜덤 생성 전)
    [SerializeField] private StructureView structurePrefab;
    [SerializeField] private IsoWorld isoWorld;                 //IsoObject 위치가 적용되려면 IsoWorld 하위에 생성해야 함
    [SerializeField] private Vector3 firstPosition;             //첫 구조물의 Iso 위치
    [SerializeField] private Vector3 spacing = new Vector3(2f, 0f, 0f);   //구조물 간 Iso 간격

    private readonly Dictionary<int, StructureView> structureCache = new();   //스테이지 번호(1부터) → 구조물

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
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        StructureView selectable = GetSelectableStructure();
        if (selectable != null && selectable.IsMouseOver(mouseWorld))
        {
            SceneManager.LoadScene(selectable.Data.SceneName);
        }
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
