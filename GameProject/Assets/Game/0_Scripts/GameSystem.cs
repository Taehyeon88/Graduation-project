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

    public StageData CurrentStageData
    {
        get { return stageDatas[CurrentStageLevel - 1]; }
    }
    [SerializeField] private StageData[] stageDatas;           //스테이지 데이터 배열


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

        Skill[] cards = RewardSystem.Instance.GetRewards(3);

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

    public void GoToNextLevel()
    {
        CurrentStageLevel++;
        IsGameClear = false;

        if (CurrentStageLevel > stageDatas.Length)
        {
            UISystem.Instance.EndDemoUI();
            return;
        }

        SceneManager.LoadScene("GameDemoScene");
    }

    public void StartFromScratch()
    {
        InitializeGameData();
        SceneManager.LoadScene("GameDemoScene");
    }

    //게임 내, 저장된 모든 데이터 초기화
    public void InitializeGameData()
    {
        CurrentStageLevel = 1;
        IsGameOver = false;
        IsGameClear = false;
    }
}
