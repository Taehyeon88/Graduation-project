using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MatchSetupSystem : MonoBehaviour
{
    private HeroData heroData => GameSystem.Instance.HeroData;
    private IReadOnlyList<CardData> deck => GameSystem.Instance.Deck;
    private RoomData roomData => GameSystem.Instance.CurrentRoomData;

    private readonly int drawCount = 5;

    private void Start()
    {
        StartCoroutine(StartSetting());
    }

    private IEnumerator StartSetting()
    {
        int MainBgmId = 32;
        int BossBgmId = 33;
        SoundSystem.Instance.PlayBGM(roomData.IsBossRoom ? BossBgmId : MainBgmId);

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

        //맵 랜덤 생성
        if (roomData.Custom_Set_Obj)
        {
            TokenSystem.Instance.StartSetObstacles(roomData.ObstacleDatas, roomData.ObstacleSetUpPositions.ToList());
        }
        else
        {
            RandomMapCreator.Instance.CreateMap(roomData.MapThemeData, roomData.HeroSetUpPositions.ToArray());
        }

        //웨이브 시스템(몬스터 배치)
        yield return WaveSystem.Instance.SetUp(roomData.WaveData.EnemyDatas, roomData.WaveData.EnemyCountsPerWave.ToList());

        //영웅 배치
        TokenSystem.Instance.StartSetHero(heroData, roomData.HeroSetUpPositions.ToArray());
        yield return new WaitUntil(() => TokenSystem.Instance.HeroView != null);
        InteractionSystem.Instance.EndInteraction();

        //기타
        CardSystem.Instance.SetUp(new(deck));
        //PerkSystem.Instance.AddPerk(new(perkData));

        StartBattleGA startBattleGA = new();
        ActionSystem.Instance.Perform(startBattleGA);
    }
}
