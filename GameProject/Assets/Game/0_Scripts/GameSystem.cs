using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSystem : Singleton<GameSystem>
{
    [field : SerializeField] public HeroData HeroData { get; private set; } //영웅 데이터
    public int CurrentLevel { get; private set; } = 1;                      //현재 층 수
    public bool IsGameClear { get; private set; }                           //게임 종료
    public bool IsTutorial { get; private set; } = true;                    //튜토리얼
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


    [SerializeField] private List<CardData> deck;                     //카드 덱
    [SerializeField] private RoomData[] roomDatas;                    //방 데이터
    [SerializeField] private RoomData tutorialRoomData;               //튜토리얼 방 데이터
    [SerializeField] private List<CardData> tutorilaDeck;             //튜토리얼 덱

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad();

        //첫 인스턴스만 체인처리
        ActionSystem.AttachPerformer<GameClearGA>(GameClearPerformer);
    }
    private void OnDisable()
    {
        if (Instance != this)
            ActionSystem.DetachPerformer<GameClearGA>();
    }

    //Performers
    private IEnumerator GameClearPerformer(GameClearGA gameClearGA)
    {
        Debug.Log("게임 클리어");
        IsGameClear = true;

        Card[] cards = RewardSystem.Instance.GetRewards(3);
        UISystem.Instance.UpdateRewardCards(cards);

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
            Debug.LogError($"다음 층수 : {CurrentLevel}이고 현재 구현된 층수는 {CurrentLevel - 1}으로 최대 층수에 도달했습니다.");

        SceneManager.LoadScene(0);
    }

    //Tests
    public void SetCurrentRoomData(RoomData roomData)
    {
        CurrentLevel = 1;
        roomDatas[0] = roomData;
        IsGameClear = false;

        SceneManager.LoadScene(0);
    }
}
