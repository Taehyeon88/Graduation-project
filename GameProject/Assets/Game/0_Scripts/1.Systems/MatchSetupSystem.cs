using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MatchSetupSystem : MonoBehaviour
{
    private HeroData heroData => GameSystem.Instance.HeroData;
    private IReadOnlyList<CardData> deck => GameSystem.Instance.Deck;
    private List<TokenData> enemyDatas => new(GameSystem.Instance.EnemyDatas);
    private List<TokenData> obstacleDatas => new(GameSystem.Instance.ObstacleDatas);
    private List<Vector2Int> heroSetUpPositions => new(GameSystem.Instance.HeroSetUpPositions);
    private List<Vector2Int> enemySetUpPositions => new(GameSystem.Instance.EnemySetUpPositions);
    private List<Vector2Int> obstacleSetUpPositions => new(GameSystem.Instance.ObstacleSetUpPositions);
    private readonly int drawCount = 5;
    private void Start()
    {
        StartCoroutine(StartSetting());
    }

    private IEnumerator StartSetting()
    {
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

        //몬스터 배치
        TokenSystem.Instance.StartSettingEnemys(enemyDatas, enemySetUpPositions);
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
