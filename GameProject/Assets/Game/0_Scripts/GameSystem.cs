using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSystem : Singleton<GameSystem>
{
    private const string StageSceneName = "StageScene";

    public int CurrentStageLevel { get; private set; } = 1;                      //지금 플레이하는(마지막으로 들어간) 스테이지 번호 (스테이지 선택 순서, 1부터)
    public bool IsGameClear { get; private set; }                           //게임 클리어
    public bool IsGameOver { get; private set; }                            //게임 오버
    public bool IsTutorial { get; set; } = false;                            //튜토리얼                               //카드 덱

    [SerializeField] private List<HeroData> startingDeck;                    //초기 덱 (인스펙터 설정)
    public List<Hero> Deck { get; private set; } = new();                    //보유 영웅 카드 (씬 전환 후에도 유지, 영웅별 가변 상태 보유)
    public List<Hero> BattleDeck { get; private set; } = new();              //이번 전투의 시작 덱 = 기본 제공 카드 + 선택 카드
    public IReadOnlyList<Hero> BaseHeroes => baseHeroes;                     //시작 덱 중 스테이지 기본 제공 카드 (진입마다 새로 생성)
    [field: SerializeField] public List<PerkData> Perks { get; private set; } //유저 소유 유물 (씬 전환 후에도 유지)

    public StructureData CurrentStructure { get; private set; }   //스테이지 선택 씬에서 고른 구조물
    public StageData CurrentStageData => CurrentStructure.StageData;
    public StageProgress CurrentProgress => GetProgress(CurrentStructure);

    public int Stars { get; private set; }       //획득한 별 총량 (공용 트리 재화)
    public int Insignias { get; private set; }   //획득한 휘장 총량 (영웅 승급 재화)

    public IReadOnlyList<bool> LastAchievedStars { get; private set; } = new bool[StageProgress.StarCount];   //마지막 승리 판의 ★1~★3 충족 여부 (승리 UI 표시용)
    public int LastGainedStars { get; private set; }       //마지막 승리로 새로 얻은 별
    public int LastGainedInsignias { get; private set; }   //마지막 승리로 얻은 휘장

    [field: SerializeField] public HeroCaptainData HeroCaptainData { get; private set; }   //용병대장(아군 코어) 초기 데이터
    [field: SerializeField] public EnemyFlagData EnemyFlagData { get; private set; }       //적 세력 깃발(적 코어) 데이터

    private readonly List<Hero> baseHeroes = new();
    private readonly Dictionary<StructureData, StageProgress> progresses = new();   //구조물별 진행 기록
    private readonly Dictionary<HeroData, HeroPromotion> promotions = new();        //영웅 종류별 승급 상태 (보유 영웅만)
    private readonly Dictionary<GrowthNodeData, int> nodeLevels = new();            //공용 트리 노드별 구매 레벨 (1레벨 이상만)


    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;

        InitializeGameData();  //게임 데이터 초기화


        //첫 인스턴스만 체인처리
        ActionSystem.AttachPerformer<GameClearGA>(GameClearGAPerformer);
        ActionSystem.AttachPerformer<GameOverGA>(GameOverGAPerformer);

        //각 영웅 데이터 값 초기화
    }

    private void OnDisable()
    {
        if (Instance != this) return;

        ActionSystem.DetachPerformer<GameClearGA>();
        ActionSystem.DetachPerformer<GameOverGA>();

    }
    //Performers
    private IEnumerator GameClearGAPerformer(GameClearGA gameClearGA)
    {
        IsGameClear = true;

        //클리어 기록 (별·휘장·해금) — 재화는 승리 UI가 뜨기 전에 바로 반영
        bool[] achievedStars = StarSystem.Instance != null
            ? StarSystem.Instance.EvaluateStars()
            : new[] { true, false, false };   //StarSystem이 없는 씬이면 ★1(승리)만
        RecordClear(achievedStars);

        //영웅·몬스터 배치 가능 구역 VG 제거
        VisualGridCreator.Instance.RemoveVisualGrid(HeroSystem.Instance.gameObject.GetInstanceID(), "Zone_Hero");
        VisualGridCreator.Instance.RemoveVisualGrid(EnemySystem.Instance.gameObject.GetInstanceID(), "Zone_Enemy");

        //그리드 위 모든 토큰(영웅, 몬스터, 장애물)과 지형지물 제거
        TokenSystem.Instance.Main.ClearAllTokens();
        TokenSystem.Instance.Setup.ClearTerrains();

        if (UISystem.Instance != null)
        {
            UISystem.Instance.OnGameClearUI();
        }

        yield return null;
    }

    private IEnumerator GameOverGAPerformer(GameOverGA gameOverGA)
    {
        IsGameOver = true;

        if (UISystem.Instance != null)
        {
            UISystem.Instance.OnGameOverUI();
        }
        yield return null;
    }

    //Publics
    //보상으로 선택한 용병을 덱에 추가
    public void AddHeroToDeck(HeroData heroData)
    {
        if (heroData != null)
            Deck.Add(new Hero(heroData, GetPromotion(heroData)));
    }

    //영웅 종류의 승급 상태 (없으면 새로 만듦) — 같은 종류의 보유 카드는 모두 이 상태를 공유
    public HeroPromotion GetPromotion(HeroData heroData)
    {
        if (!promotions.TryGetValue(heroData, out HeroPromotion promotion))
        {
            promotion = new HeroPromotion(heroData);
            promotions.Add(heroData, promotion);
        }
        return promotion;
    }

    //휘장 사용 (부족하면 쓰지 않고 false)
    public bool TrySpendInsignias(int amount)
    {
        if (amount < 0 || Insignias < amount) return false;
        Insignias -= amount;
        return true;
    }

    //공용 트리 노드 구매 레벨 (규칙 판단은 GrowthNodeSystem)
    public int GetNodeLevel(GrowthNodeData node)
        => node != null && nodeLevels.TryGetValue(node, out int level) ? level : 0;

    public void AddNodeLevel(GrowthNodeData node)
    {
        nodeLevels[node] = GetNodeLevel(node) + 1;
    }

    public void ClearNodeLevels()
    {
        nodeLevels.Clear();
    }

    //구매한 전체 레벨 합 (= 쓴 별, 1레벨 = 별 1)
    public int TotalNodeLevels => nodeLevels.Values.Sum();

    //해당 효과를 가진 노드들의 레벨 × 레벨당 효과량 합 (전투 시작 시 각 적용 지점이 조회)
    public int GetGrowthBonus(GrowthNodeType type)
        => nodeLevels.Where(pair => pair.Key.Type == type).Sum(pair => pair.Value * pair.Key.AmountPerLevel);

    public StageProgress GetProgress(StructureData structure)
    {
        if (!progresses.TryGetValue(structure, out StageProgress progress))
        {
            progress = new StageProgress();
            progresses.Add(structure, progress);
        }
        return progress;
    }

    //선택 슬롯 수 = 스테이지 덱 크기 - 기본 제공 카드 수 + 공용 트리 덱 슬롯 (상한은 미정)
    public int GetSelectSlotCount(StageData stage)
        => Mathf.Max(0, stage.DeckSize - (stage.BaseHeroes?.Length ?? 0)) + GetGrowthBonus(GrowthNodeType.DECK_SLOT);

    //기본 제공 카드와 같은 영웅은 중복 선택 불가
    public bool CanSelect(StageData stage, Hero hero)
        => stage.BaseHeroes == null || !stage.BaseHeroes.Contains(hero.Data);

    //지난 선택(없으면 보유 카드 앞에서부터)으로 슬롯을 채운 선택 — 디버그 실행 등 팝업을 거치지 않는 진입용
    public List<Hero> GetAutoSelection(StructureData structure)
    {
        StageData stage = structure.StageData;
        if (stage == null) return new List<Hero>();

        IReadOnlyList<Hero> last = GetProgress(structure).LastSelection;
        IEnumerable<Hero> source = last.Count > 0 ? last : Deck;
        return source.Where(hero => Deck.Contains(hero) && CanSelect(stage, hero))
                     .Take(GetSelectSlotCount(stage))
                     .ToList();
    }

    //구조물 진입 준비 (씬 이동은 호출한 쪽에서) — 전투 구조물이면 시작 덱 구성·지난 선택 기록
    public void EnterStage(StructureData structure, int stageNumber, IReadOnlyList<Hero> selection)
    {
        CurrentStructure = structure;
        CurrentStageLevel = stageNumber;
        IsGameClear = false;
        IsGameOver = false;

        baseHeroes.Clear();
        BattleDeck.Clear();
        if (structure.StageData == null) return;

        if (structure.StageData.BaseHeroes != null)
            baseHeroes.AddRange(structure.StageData.BaseHeroes.Where(data => data != null).Select(data => new Hero(data)));

        BattleDeck.AddRange(baseHeroes);
        BattleDeck.AddRange(selection);
        GetProgress(structure).SetLastSelection(selection);
    }

    //패배 후 같은 스테이지·같은 선택으로 재도전
    public void RetryStage()
    {
        List<Hero> selection = new(CurrentProgress.LastSelection);
        EnterStage(CurrentStructure, CurrentStageLevel, selection);
        FadeSystem.Instance.LoadSceneWithFade(CurrentStructure.SceneName);
    }

    public void ReturnToStageSelect()
    {
        IsGameClear = false;
        IsGameOver = false;
        FadeSystem.Instance.LoadSceneWithFade(StageSceneName);
    }

    //비전투 구조물(주점 등)을 마쳤을 때 — 별 없이 클리어 처리해 다음 해금
    public void MarkCurrentCleared()
    {
        CurrentProgress.MarkCleared();
    }

    public void StartFromScratch()
    {
        InitializeGameData();
        FadeSystem.Instance.LoadSceneWithFade(StageSceneName);
    }

    //게임 내, 저장된 모든 데이터 초기화
    public void InitializeGameData()
    {
        CurrentStageLevel = 1;
        CurrentStructure = null;
        IsGameOver = false;
        IsGameClear = false;
        promotions.Clear();
        Deck = startingDeck.Where(d => d != null).Select(d => new Hero(d, GetPromotion(d))).ToList();
        BattleDeck.Clear();
        baseHeroes.Clear();
        progresses.Clear();
        nodeLevels.Clear();
        Stars = 0;
        Insignias = 0;
    }

    //Privates
    //승리 기록: 클리어 처리, 새로 달성한 별만 지급, 휘장은 첫 클리어 1회
    private void RecordClear(bool[] achievedStars)
    {
        StageProgress progress = CurrentProgress;
        progress.MarkCleared();

        LastAchievedStars = achievedStars;
        LastGainedStars = progress.RecordStars(achievedStars);
        LastGainedInsignias = progress.TryGiveInsignia() ? CurrentStageData.InsigniaReward : 0;

        Stars += LastGainedStars;
        Insignias += LastGainedInsignias;
    }
}
