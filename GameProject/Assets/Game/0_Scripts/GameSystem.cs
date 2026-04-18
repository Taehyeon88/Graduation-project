using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSystem : Singleton<GameSystem>
{
    [field : SerializeField] public HeroData HeroData { get; private set; }
    public IReadOnlyList<CardData> Deck { get { return deck;} }                //플레이어 덱
    public IReadOnlyList<EnemyData> EnemyDatas => CurrentRoomData.enemyDatas;
    public IReadOnlyList<WallData> WallDatas => CurrentRoomData.wallDatas;
    public IReadOnlyList<Vector2Int> HeroSetUpPositions => CurrentRoomData.heroSetUpPositions;
    public IReadOnlyList<Vector2Int> EnemySetUpPositions => CurrentRoomData.enemySetUpPositions;
    public IReadOnlyList<Vector2Int> WallSetUpPositions => CurrentRoomData.wallSetUpPositions;

    public int CurrentLevel { get; private set; } = 1;                                 //현재 층 수
    public RoomData CurrentRoomData { get { return roomDatas[CurrentLevel - 1]; } }    //현재 층 데이터
    public bool IsGameClear { get; private set; }                                      //게임 종료

    [SerializeField] private List<CardData> deck;                                      //카드 덱
    [SerializeField] private RoomData[] roomDatas = new RoomData[1];                   //스테이지 데이터

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad();
    }

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<GameClearGA>(GameClearPerformer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<GameClearGA>();
    }

    private IEnumerator GameClearPerformer(GameClearGA gameClearGA)
    {
        IsGameClear = true;

        Card[] cards = RewardSystem.Instance.GetRewards(3);
        UISystem.Instance.UpdateRewardCards(cards);

        yield return null;
    }

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

        if (CurrentLevel >= roomDatas.Length)
            Debug.LogError($"다음 층수 : {CurrentLevel}이고 현재 구현된 층수는 {CurrentLevel - 1}으로 최대 층수에 도달했습니다.");

        SceneManager.LoadScene(0);
    }
}
