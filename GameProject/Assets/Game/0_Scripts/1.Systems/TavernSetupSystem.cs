using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TavernSetupSystem : MonoBehaviour   //주점 씬 1회성 부트스트랩: 소문 없는 영웅을 손패에 올려 단일 타일에 배치
{
    private static readonly Vector2Int TilePos = Vector2Int.zero;   //주점의 유일한 타일

    private void Start()
    {
        StartCoroutine(StartSetting());
    }

    private IEnumerator StartSetting()
    {
        //1.타일 1개짜리 맵 생성
        TokenSystem.Instance.Setup.SetUpStageMap();

        //2.영웅 배치 가능 위치 = 유일한 타일
        HeroSystem.Instance.SetUp(new List<Vector2Int> { TilePos });

        //2.2 배치 가능 구역 상시 표시 (씬을 떠날 때까지 유지, 씬 전환 때 함께 사라짐)
        VisualGridCreator.Instance.CreateVisualGrid(HeroSystem.Instance.gameObject.GetInstanceID(), TilePos, "Zone_Hero");

        //3.AP 없이 배치
        CardSystem.Instance.FreePlacement = true;

        //4.소문이 없는 영웅 전원을 손패로
        List<Hero> heroes = GameSystem.Instance.Deck.Where(hero => hero.CanAddRumor).ToList();
        CardSystem.Instance.SetUp(heroes);
        ActionSystem.Instance.Perform(new DrawCardsGA(heroes.Count));

        yield return null;
    }
}
