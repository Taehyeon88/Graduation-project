using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MatchSetupSystem : MonoBehaviour
{
    [SerializeField] private HeroData heroData;
    [SerializeField] private PerkData perkData;
    [SerializeField] private List<EnemyData> enemyDatas;
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

        TokenManager.Instance.StartSetting(heroData);

        yield return new WaitUntil(() => !TokenManager.Instance.startSetting);

        EnemySystem.Instance.SetUp(enemyDatas);
        HeroSystem.Instance.SetUp(heroData);
        CardSystem.Instance.SetUp(heroData.Deck.ToList());
        PerkSystem.Instance.AddPerk(new(perkData));
        DrawCardsGA drawCardsGA = new(drawCount);
        ActionSystem.Instance.Perform(drawCardsGA);
    }
}
