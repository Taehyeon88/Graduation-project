using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MatchSetupSystem : MonoBehaviour
{
    [SerializeField] private HeroData heroData;
    [SerializeField] private PerkData perkData;
    [SerializeField] private List<EnemyData> enemyDatas;
    [SerializeField] private List<Vector2Int> heroSetUpPositions;
    [SerializeField] private List<Vector2Int> enemySetUpPositions;
    private readonly int drawCount = 5;
    private void Start()
    {
        StartCoroutine(StartSetting());
    }

    private IEnumerator StartSetting()
    {
        if (heroData.Deck == null)
        {
            Debug.LogError("heroData에 deckData가 설정되지 않았습니다.");
            yield break;
        }
        if (heroData.Deck.Count < drawCount)
        {
            Debug.LogError($"heroData에 설정된 deckData개수가 {drawCount}개 보다 적습니다.");
            yield break;
        }

        //건물 배치
        //몬스터 배치
        TokenSystem.Instance.StartSettingEnemys(new(enemyDatas), enemySetUpPositions);
        //아이템 배치

        //영웅 배치
        TokenSystem.Instance.StartSetHero(heroData, heroSetUpPositions);
        yield return new WaitUntil(() => TokenSystem.Instance.HeroView != null);
        //영웅 이동
        TokenSystem.Instance.StartHeroMove();

        //기타
        CardSystem.Instance.SetUp(heroData.Deck.ToList());
        PerkSystem.Instance.AddPerk(new(perkData));
        DrawCardsGA drawCardsGA = new(drawCount);
        ActionSystem.Instance.Perform(drawCardsGA);
    }
}
