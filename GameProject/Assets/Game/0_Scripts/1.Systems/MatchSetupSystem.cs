using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MatchSetupSystem : MonoBehaviour
{
    [SerializeField] AllPerksDisPlayUI allPerksDisplayUI;
    private HeroData[] heroDatas => GameSystem.Instance.HeroDatas;
    private StageData stageData => GameSystem.Instance.CurrentStageData;


    private void Start()
    {
        StartCoroutine(StartSetting());
    }

    private IEnumerator StartSetting()
    {
        //1.브금 실행
        SoundSystem.Instance.PlaySound(stageData.StageBGMId);

        //2.스테이지 맵 생성
        TokenSystem.Instance.Setup.SetUpStageMap();
        //3.스테이지 기물 배치(잠깐 패스)

        //4.몬스터 및 웨이브 핵 배치
        WaveSystem.Instance.SetUp(
            stageData.waveData.EnemyDatas.ToArray(),
            stageData.waveData.WavePerEnemyCount.ToArray(),
            stageData.waveData.WaveTurnIntervals.ToArray(),
            stageData.waveData.WaveCoreData
            );
        WaveSystem.Instance.SetUpFirstEnemys();

        //4.5 영웅 아이템 정보 UI 설정
        allPerksDisplayUI.SetUp(heroDatas.ToArray());

        //5.플레이어 유닛 카드 덱 구성 (유닛 배치는 전투 중 카드로 진행)
        CardSystem.Instance.SetUp();

        //6.전투 시작(Event)
        TurnGA turnGA = new(TurnType.StartBattle);
        ActionSystem.Instance.Perform(turnGA);

        yield return null;
    }
}
