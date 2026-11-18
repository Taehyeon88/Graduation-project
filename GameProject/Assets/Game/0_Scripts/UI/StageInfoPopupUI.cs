using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//스테이지 정보 팝업: 용병대장·적 깃발 체력, 등장 몬스터, 별 조건, 맵 미리보기, 영웅 카드 선택 → 진입
//반복 요소(몬스터·맵 칸·카드)는 비활성 템플릿을 복제해 채운다
public class StageInfoPopupUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;

    [Header("스테이지 정보")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text captainHealthText;
    [SerializeField] private TMP_Text flagHealthText;

    [Header("등장 몬스터")]
    [SerializeField] private RectTransform monsterContent;
    [SerializeField] private Image monsterTemplate;        //몬스터 아이콘 템플릿 (자식 TMP_Text에 이름)

    [Header("별 조건 (★1~★3 순서)")]
    [SerializeField] private TMP_Text[] starTexts;
    [SerializeField] private Image[] starMarks;            //달성 여부 표시
    [SerializeField] private Sprite filledStarSprite;
    [SerializeField] private Sprite emptyStarSprite;

    [Header("맵 미리보기")]
    [SerializeField] private RectTransform mapContent;     //GridLayoutGroup (열 = 그리드 가로)
    [SerializeField] private Image mapCellTemplate;
    [SerializeField] private int gridSize = 4;             //전투판 4×4 (combat-rules.md §1)
    [SerializeField] private Color emptyCellColor = new(0.85f, 0.85f, 0.85f, 1f);
    [SerializeField] private Color neutralCellColor = new(0.35f, 0.35f, 0.35f, 1f);
    [SerializeField] private Color obstacleCellColor = new(0.6f, 0.4f, 0.2f, 1f);
    [SerializeField] private Color terrainCellColor = new(0.3f, 0.7f, 0.3f, 1f);

    [Header("영웅 카드 선택")]
    [SerializeField] private RectTransform baseCardContent;     //기본 제공 카드 (고정)
    [SerializeField] private RectTransform ownedCardContent;    //보유 카드 (선택)
    [SerializeField] private HeroCardSlotUI cardSlotTemplate;
    [SerializeField] private TMP_Text slotText;                 //선택 n / 슬롯 m
    [SerializeField] private TMP_Text warningText;              //빈 슬롯 경고

    [Header("버튼")]
    [SerializeField] private Button enterButton;
    [SerializeField] private Button closeButton;

    public bool IsOpen => panel.activeSelf;

    private readonly List<GameObject> spawned = new();
    private readonly List<HeroCardSlotUI> ownedSlots = new();
    private readonly List<Hero> selection = new();
    private int slotCount;
    private Action<List<Hero>> entered;

    private void Awake()
    {
        panel.SetActive(false);
        monsterTemplate.gameObject.SetActive(false);
        mapCellTemplate.gameObject.SetActive(false);
        cardSlotTemplate.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        enterButton.onClick.AddListener(Enter);
        closeButton.onClick.AddListener(Close);
    }

    private void OnDisable()
    {
        enterButton.onClick.RemoveListener(Enter);
        closeButton.onClick.RemoveListener(Close);
    }

    //Publics
    //전투 구조물 정보를 채워 열기. 진입 시 선택한 영웅 카드를 onEnter로 넘긴다
    public void Open(StructureData structure, Action<List<Hero>> onEnter)
    {
        GameSystem game = GameSystem.Instance;
        StageData stage = structure.StageData;
        StageProgress progress = game.GetProgress(structure);

        Clear();
        entered = onEnter;

        titleText.SetText(structure.name);
        captainHealthText.SetText($"용병대장 체력 {stage.HeroCaptainHealth + GameSystem.Instance.GetGrowthBonus(GrowthNodeType.CAPTAIN_HEALTH)}");   //공용 트리 보너스 포함
        flagHealthText.SetText($"적 깃발 체력 {stage.EnemyCoreHealth}");

        FillMonsters(stage);
        FillStars(stage, progress);
        FillMap(stage);
        FillCards(game, stage, progress);

        panel.SetActive(true);
    }

    public void Close()
    {
        panel.SetActive(false);
        Clear();
        entered = null;
    }

    //Privates
    private void FillMonsters(StageData stage)
    {
        if (stage.waveData == null || stage.waveData.EnemyDatas == null) return;

        foreach (EnemyData enemy in stage.waveData.EnemyDatas.Where(e => e != null).Distinct())
        {
            Image icon = Spawn(monsterTemplate, monsterContent);
            icon.sprite = enemy.Sprite;
            icon.preserveAspect = true;
            TMP_Text label = icon.GetComponentInChildren<TMP_Text>(true);   //팝업이 아직 닫힌 상태에서 채우므로 비활성 포함
            if (label != null) label.SetText(enemy.Name);
        }
    }

    private void FillStars(StageData stage, StageProgress progress)
    {
        for (int i = 0; i < starTexts.Length && i < StageProgress.StarCount; i++)
        {
            starTexts[i].SetText(StarSystem.GetStarDescription(stage, i));
            starMarks[i].sprite = progress.HasStar(i) ? filledStarSprite : emptyStarSprite;
        }
    }

    //y=0이 위쪽 줄 (combat-rules.md §2 표기와 같은 방향)
    private void FillMap(StageData stage)
    {
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                Vector2Int pos = new(x, y);
                Image cell = Spawn(mapCellTemplate, mapContent);
                cell.color = GetCellColor(stage, pos);
            }
        }
    }

    private Color GetCellColor(StageData stage, Vector2Int pos)
    {
        if (stage.NeutralPositions != null && stage.NeutralPositions.Contains(pos)) return neutralCellColor;
        if (stage.ObstacleSpawns != null && stage.ObstacleSpawns.Any(o => o.Position == pos)) return obstacleCellColor;
        if (stage.TerrainSpawns != null && stage.TerrainSpawns.Any(t => t.Position == pos)) return terrainCellColor;
        return emptyCellColor;
    }

    private void FillCards(GameSystem game, StageData stage, StageProgress progress)
    {
        slotCount = game.GetEffectiveSlotCount(stage);

        if (stage.BaseHeroes != null)
        {
            foreach (HeroData data in stage.BaseHeroes.Where(d => d != null))
                Spawn(cardSlotTemplate, baseCardContent).SetUp(new Hero(data), true, null);
        }

        foreach (Hero hero in game.Deck)
        {
            HeroCardSlotUI slot = Spawn(cardSlotTemplate, ownedCardContent);
            slot.SetUp(hero, !game.CanSelect(stage, hero), ToggleSelect);
            slot.gameObject.AddComponent<TutorialTarget>().Configure($"stage_hero_{hero.Data.Id}", false);   //튜토리얼이 특정 영웅 카드를 지목할 수 있게 (Open마다 재생성)
            ownedSlots.Add(slot);
        }

        //지난 선택 미리 채움 (보유 중이고 선택 가능한 것만, 슬롯 수까지)
        foreach (Hero hero in progress.LastSelection)
        {
            if (selection.Count >= slotCount) break;
            if (!game.Deck.Contains(hero) || !game.CanSelect(stage, hero)) continue;
            selection.Add(hero);
        }
        RefreshSelection();
    }

    private void ToggleSelect(HeroCardSlotUI slot)
    {
        if (selection.Contains(slot.Hero))
            selection.Remove(slot.Hero);
        else if (selection.Count < slotCount)
            selection.Add(slot.Hero);

        RefreshSelection();
    }

    private void RefreshSelection()
    {
        foreach (HeroCardSlotUI slot in ownedSlots)
            slot.SetSelected(selection.Contains(slot.Hero));

        slotText.SetText($"선택 {selection.Count} / {slotCount}");

        int emptySlots = slotCount - selection.Count;
        warningText.gameObject.SetActive(emptySlots > 0);
        warningText.SetText($"슬롯을 모두 채워야 진입할 수 있습니다 (빈 슬롯 {emptySlots}개)");

        enterButton.interactable = emptySlots <= 0;   //슬롯을 모두 채워야 진입
    }

    private void Enter()
    {
        if (selection.Count < slotCount) return;

        Action<List<Hero>> onEnter = entered;
        List<Hero> chosen = new(selection);
        Close();
        onEnter?.Invoke(chosen);
    }

    private T Spawn<T>(T template, RectTransform parent) where T : Component
    {
        T instance = Instantiate(template, parent);
        instance.gameObject.SetActive(true);
        spawned.Add(instance.gameObject);
        return instance;
    }

    private void Clear()
    {
        foreach (GameObject go in spawned)
            Destroy(go);
        spawned.Clear();
        ownedSlots.Clear();
        selection.Clear();
    }
}
