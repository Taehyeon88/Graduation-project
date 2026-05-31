using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MatchSetupSystem : MonoBehaviour
{
    private HeroData heroData => GameSystem.Instance.HeroData;
    private IReadOnlyList<CardData> deck => GameSystem.Instance.Deck;
    private IReadOnlyList<EnemyData> enemyDatas => roomData.enemyDatas.ToList();
    private List<TokenData> obstacleDatas => new(roomData.obstacleDatas);
    private List<Vector2Int> heroSetUpPositions => new(roomData.heroSetUpPositions);
    private IReadOnlyList<int> enemyCountsPerWave => roomData.enemyCountsPerWave;
    private List<Vector2Int> obstacleSetUpPositions => new(roomData.obstacleSetUpPositions);
    private readonly int drawCount = 5;

    private RoomData roomData;
    private void Start()
    {
        StartCoroutine(StartSetting());
    }

    private IEnumerator StartSetting()
    {
        roomData = GameSystem.Instance.CurrentRoomData;

        if (deck == null)
        {
            Debug.LogError("GameManager에 deckData가 설정되지 않았습니다.");
            yield break;
        }
        if (deck.Count < drawCount)
        {
            Debug.LogError($"GameManager에 설정된 deckData개수가 {drawCount}개 보다 적습니다.");
            yield break;
        }

        //장애물 배치
        TokenSystem.Instance.StartSetObstacles(obstacleDatas, obstacleSetUpPositions);

        //웨이브 시스템(몬스터 배치)
        yield return WaveSystem.Instance.SetUp(enemyDatas, enemyCountsPerWave);
        //아이템 배치

        //영웅 배치
        TokenSystem.Instance.StartSetHero(heroData, heroSetUpPositions);
        yield return new WaitUntil(() => TokenSystem.Instance.HeroView != null);
        InteractionSystem.Instance.EndInteraction();

        //기타
        CardSystem.Instance.SetUp(new(deck));
        //PerkSystem.Instance.AddPerk(new(perkData));

        StartBattleGA startBattleGA = new();
        ActionSystem.Instance.Perform(startBattleGA);
    }
}
