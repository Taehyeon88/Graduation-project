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

    [field: SerializeField] public List<HeroData> Deck { get; private set; } //덱 (씬 전환 후에도 유지)
    [field: SerializeField] public List<PerkData> Perks { get; private set; } //유저 소유 유물 (씬 전환 후에도 유지)

    public StageData CurrentStageData
    {
        get { return stageDatas[CurrentStageLevel - 1]; }
    }
    [SerializeField] private StageData[] stageDatas;           //스테이지 데이터 배열

    [field: SerializeField] public HeroCaptainData HeroCaptainData { get; private set; }   //용병대장(아군 코어) 초기 데이터
    [field: SerializeField] public EnemyFlagData EnemyFlagData { get; private set; }       //적 세력 깃발(적 코어) 데이터


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
            Deck.Add(heroData);
    }

    public void GoToNextLevel()
    {
        CurrentStageLevel++;
        IsGameClear = false;

        SceneManager.LoadScene("StageScene");
    }

    public void StartFromScratch()
    {
        InitializeGameData();
        SceneManager.LoadScene("StageScene");
    }

    //게임 내, 저장된 모든 데이터 초기화
    public void InitializeGameData()
    {
        CurrentStageLevel = 1;
        IsGameOver = false;
        IsGameClear = false;
    }
}
