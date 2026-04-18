using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MatchSetupSystem : MonoBehaviour
{
    private HeroData heroData => GameSystem.Instance.HeroData;
    private IReadOnlyList<CardData> deck => GameSystem.Instance.Deck;
    private List<TokenData> enemyDatas => new(GameSystem.Instance.EnemyDatas);
    private List<TokenData> wallDatas => new(GameSystem.Instance.WallDatas);
    private List<Vector2Int> heroSetUpPositions => new(GameSystem.Instance.HeroSetUpPositions);
    private List<Vector2Int> enemySetUpPositions => new(GameSystem.Instance.EnemySetUpPositions);
    private List<Vector2Int> wallSetUpPositions => new(GameSystem.Instance.WallSetUpPositions);
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

        //벽 배치
        TokenSystem.Instance.StartSetWalls(wallDatas, wallSetUpPositions);

        //몬스터 배치
        TokenSystem.Instance.StartSettingEnemys(enemyDatas, enemySetUpPositions);
        //아이템 배치

        //영웅 배치
        TokenSystem.Instance.StartSetHero(heroData, heroSetUpPositions);
        yield return new WaitUntil(() => TokenSystem.Instance.HeroView != null);
        InteractionSystem.Instance.EndInteraction();

        //기타
        CardSystem.Instance.SetUp(deck.ToList());
        //PerkSystem.Instance.AddPerk(new(perkData));

        StartBattleGA startBattleGA = new();
        ActionSystem.Instance.Perform(startBattleGA);
    }
}
