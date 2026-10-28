using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSystem : Singleton<GameSystem>
{
    public int CurrentStageLevel { get; private set; } = 1;                      //현재 층 수
    public bool IsGameClear { get; private set; }                           //게임 클리어
    public bool IsGameOver { get; private set; }                            //게임 오버
    public bool IsTutorial { get; set; } = false;                            //튜토리얼                               //카드 덱

    [SerializeField] private List<HeroData> startingDeck;                    //초기 덱 (인스펙터 설정)
    public List<Hero> Deck { get; private set; } = new();                    //덱 (씬 전환 후에도 유지, 영웅별 가변 상태 보유)
    [field: SerializeField] public List<PerkData> Perks { get; private set; } //유저 소유 유물 (씬 전환 후에도 유지)

    public StructureData CurrentStructure { get; private set; }   //스테이지 선택 씬에서 고른 구조물
    public StageData CurrentStageData => CurrentStructure.StageData;

    [field: SerializeField] public HeroCaptainData HeroCaptainData { get; private set; }   //용병대장(아군 코어) 초기 데이터
    [field: SerializeField] public EnemyFlagData EnemyFlagData { get; private set; }       //적 세력 깃발(적 코어) 데이터

    public int HeroCaptainHealth { get; private set; }      //다음 전투로 이어지는 용병대장 현재 체력 (런 단위)
    public int HeroCaptainMaxHealth { get; private set; }   //다음 전투로 이어지는 용병대장 최대 체력 (런 단위)


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

        //용병대장 체력을 다음 스테이지로 이어감
        HeroCaptainView captain = HeroSystem.Instance.HeroCaptain;
        if (captain != null)
        {
            HeroCaptainHealth = captain.CurrentHealth;
            HeroCaptainMaxHealth = captain.MaxHealth;
        }

        //영웅·몬스터 배치 가능 구역 VG 제거
        VisualGridCreator.Instance.RemoveVisualGrid(HeroSystem.Instance.gameObject.GetInstanceID(), "Zone_Hero");
        VisualGridCreator.Instance.RemoveVisualGrid(EnemySystem.Instance.gameObject.GetInstanceID(), "Zone_Enemy");

        //그리드 위 모든 토큰(영웅, 몬스터, 장애물)과 지형지물 제거
        TokenSystem.Instance.Main.ClearAllTokens();
        TokenSystem.Instance.Setup.ClearTerrains();

        //현재 스테이지 보상 용병 수령 (생성은 승리 UI 확인 후)
        HeroData[] rewards = RewardSystem.Instance.GetRewards(CurrentStageLevel);
        RewardSelectSystem.Instance.SetRewards(rewards);

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

    //보상으로 선택한 용병을 덱에 추가
    public void AddHeroToDeck(HeroData heroData)
    {
        if (heroData != null)
            Deck.Add(new Hero(heroData));
    }

    public void SelectStructure(StructureData structure)
    {
        CurrentStructure = structure;
    }

    public void GoToNextLevel()
    {
        CurrentStageLevel++;
        IsGameClear = false;

        FadeSystem.Instance.LoadSceneWithFade("StageScene");
    }

    public void StartFromScratch()
    {
        InitializeGameData();
        FadeSystem.Instance.LoadSceneWithFade("StageScene");
    }

    //게임 내, 저장된 모든 데이터 초기화
    public void InitializeGameData()
    {
        CurrentStageLevel = 1;
        CurrentStructure = null;
        IsGameOver = false;
        IsGameClear = false;
        Deck = startingDeck.Where(d => d != null).Select(d => new Hero(d)).ToList();
        HeroCaptainHealth = HeroCaptainData.Health;
        HeroCaptainMaxHealth = HeroCaptainData.Health;
    }
}
