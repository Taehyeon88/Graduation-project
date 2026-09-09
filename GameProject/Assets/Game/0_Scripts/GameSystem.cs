using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSystem : Singleton<GameSystem>
{
    public int CurrentGold { get; private set; }                            //현재 플레이어 골드
    public int CurrentStageLevel { get; private set; } = 1;                      //현재 층 수
    public bool IsGameClear { get; private set; }                           //게임 클리어
    public bool IsGameOver { get; private set; }                            //게임 오버
    public bool IsTutorial { get; set; } = false;                            //튜토리얼                               //카드 덱

    [field: SerializeField] public HeroData[] HeroDatas { get; private set; } //영웅 데이터s
    public IReadOnlyList<Hero> Heros => heros;
    private List<Hero> heros = new List<Hero>(3);       //영웅s

    public StageData CurrentStageData
    {
        get { return stageDatas[CurrentStageLevel - 1]; }
    }
    [SerializeField] private StageData[] stageDatas;           //스테이지 데이터 배열


    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;

        IntializeGameData();  //게임 데이터 초기화


        //첫 인스턴스만 체인처리
        ActionSystem.AttachPerformer<GameClearGA>(GameClearPerformer);
        ActionSystem.AttachPerformer<GameOverGA>(GameOverPerformer);

        //각 영웅 데이터 값 초기화
    }

    private void OnDisable()
    {
        if (Instance != this) return;

        ActionSystem.DetachPerformer<GameClearGA>();
        ActionSystem.DetachPerformer<GameOverGA>();
        
    }
    //Performers
    private IEnumerator GameClearPerformer(GameClearGA gameClearGA)
    {
        Debug.Log("게임 클리어");
        IsGameClear = true;

        Skill[] cards = RewardSystem.Instance.GetRewards(3);

        yield return null;
    }
    private IEnumerator GameOverPerformer(GameOverGA gameOverGA)
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
        Debug.Log(CurrentStageLevel);
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
        IntializeGameData();
        SceneManager.LoadScene("GameDemoScene");
    }

    //게임 내, 저장된 모든 데이터 초기화
    public void IntializeGameData()
    {
        CurrentStageLevel = 1;
        IsGameOver = false;
        IsGameClear = false;
        CurrentGold = 0;

        IntializeHero();
    }

    //영웅 관련 데이터 초기화
    public void IntializeHero()
    {
        CurrentGold = 0;

        foreach (var data in HeroDatas)
        {
            List<Skill> skills = new(10);
            List<PerkItem> perks = new(10);

            foreach (var skillData in data.StartingSkills)
                skills.Add(new Skill(skillData));
            foreach (var perkData in data.StartingPerks)
                perks.Add(new PerkItem(perkData));

            Hero hero = new Hero(
                data.Id, 
                data.HeroHp, 
                data.MovePoint,
                skills,
                perks
                );
            heros.Add(hero);
            data.SetHero(hero);

            CurrentGold += data.Gold;
        }
    }
}
