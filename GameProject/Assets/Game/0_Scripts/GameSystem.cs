using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSystem : Singleton<GameSystem>
{
    [field : SerializeField] public HeroData HeroData { get; private set; } //영웅 데이터
    public int MaxHp { get; private set; }                                  //현재 플레이어 최대 체력
    public int CurrentHp { get; private set; }                              //현재 플레이어 체력
    public int CurrentGold { get; private set; }                            //현재 플레이어 골드
    public int CurrentLevel { get; private set; } = 1;                      //현재 층 수
    public bool IsGameClear { get; private set; }                           //게임 클리어
    public bool IsGameOver { get; private set; }                            //게임 오버
    public bool IsTutorial { get; set; } = false;                            //튜토리얼
    public IReadOnlyList<CardData> Deck                                     //플레이어 덱
    {
        get
        {
            if(IsTutorial || TutorialSystem.Instance.IsTutorialing)
                return tutorilaDeck;
            return deck;
        }
    }
    public RoomData CurrentRoomData                                         //게임 셋업 때, 딱 1번 실행
    {
         get 
        { 
            if(IsTutorial)
            {
                IsTutorial = false;
                StartCoroutine(TutorialSystem.Instance.StartTutorial()); //튜토리얼 시작
                return tutorialRoomData;
            }
            return roomDatas[CurrentLevel - 1]; 
        } 
    }

    [SerializeField] private RoomData[] roomDatas;                    //방 데이터
    [SerializeField] private RoomData tutorialRoomData;               //튜토리얼 방 데이터
    [SerializeField] private List<CardData> tutorilaDeck;             //튜토리얼 덱
    private List<CardData> deck;                                      //카드 덱

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this) return;

        DontDestroyOnLoad();

        deck = HeroData.Deck.ToList();       //덱 데이터 받기
        MaxHp = CurrentHp = HeroData.Health; //체력 설정

        //첫 인스턴스만 체인처리
        ActionSystem.AttachPerformer<GameClearGA>(GameClearPerformer);
        ActionSystem.AttachPerformer<GameOverGA>(GameOverPerformer);
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
        CurrentHp = HeroSystem.Instance.HeroView.CurrentHealth;

        Card[] cards = RewardSystem.Instance.GetRewards(3);
        UISystem.Instance.UpdateRewardCards(cards);

        yield return null;
    }
    private IEnumerator GameOverPerformer(GameOverGA gameOverGA)
    {
        IsGameOver = true;
        CurrentHp = HeroSystem.Instance.HeroView.CurrentHealth;

        if (UISystem.Instance != null)
        {
            UISystem.Instance.OnGameOverUI();
        }
        yield return null;
    }

    //Publics
    public void AddDeckCard(CardData cardData)
    {
        deck.Add(cardData);
        UISystem.Instance.RemoveRewardCards();
    }

    public void GoToNextLevel()
    {
        CurrentLevel++;
        Debug.Log(CurrentLevel);
        IsGameClear = false;

        if (CurrentLevel > roomDatas.Length)
        {
            UISystem.Instance.EndDemoUI();
            return;
        }

        SceneManager.LoadScene("GameDemoScene");
    }

    public void StartFromScratch()
    {
        CurrentLevel = 1;
        IsGameOver = false;
        IsGameClear = false;
        MaxHp = CurrentHp = HeroData.Health;
        CurrentGold = HeroData.Gold;

        deck = HeroData.Deck.ToList();

        SceneManager.LoadScene("GameDemoScene");
    }
}
