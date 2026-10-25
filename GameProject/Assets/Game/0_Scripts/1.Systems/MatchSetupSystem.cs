using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MatchSetupSystem : MonoBehaviour
{
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

        //3.2 중립지대 설정 (이동·배치 불가 칸)
        TokenSetup tokenSetup = TokenSystem.Instance.Setup;
        tokenSetup.SetUpNeutralZones(stageData.NeutralPositions);

        //3.3 파괴 가능 장애물 배치
        tokenSetup.SetUpObstacles(stageData.ObstacleSpawns);

        //3.4 발판 배치
        tokenSetup.SetUpTerrains(stageData.TerrainSpawns);

        //3.5 진영별 라인 배치 위치 설정 (중립지대가 기본 배치 칸에 있으면 한 칸 앞으로)
        HeroSystem.Instance.SetUp(
            tokenSetup.CalcSetupPoses(stageData.NeutralPositions, 0, 1),
            tokenSetup.CalcNeutralSetupPoses(stageData.NeutralPositions, 0));
        EnemySystem.Instance.SetUp(tokenSetup.CalcSetupPoses(stageData.NeutralPositions, TokenSystem.Instance.gridWidth - 1, -1));

        //3.6 진영별 배치 구역 상시 표시 (카드 선택 VG와 별개로 전투 내내 유지)
        foreach (var pos in HeroSystem.Instance.SetupPoses)
            VisualGridCreator.Instance.CreateVisualGrid(HeroSystem.Instance.gameObject.GetInstanceID(), pos, "Zone_Hero");
        foreach (var pos in EnemySystem.Instance.SetupPoses)
            VisualGridCreator.Instance.CreateVisualGrid(EnemySystem.Instance.gameObject.GetInstanceID(), pos, "Zone_Enemy");

        //4.몬스터 및 웨이브 핵 배치
        WaveSystem.Instance.SetUp(
            stageData.waveData.EnemyDatas.ToArray(),
            stageData.waveData.WavePerEnemyCount.ToArray(),
            stageData.waveData.WaveTurnIntervals.ToArray(),
            stageData.waveData.SpawnLines
            );
        WaveSystem.Instance.SetUpFirstEnemys();

        //4.2 용병대장/적 세력 깃발 View 생성 (그리드 밖)
        tokenSetup.SetUpHeroCaptain(GameSystem.Instance.HeroCaptainData);
        tokenSetup.SetUpEnemyFlag(GameSystem.Instance.EnemyFlagData, stageData.EnemyCoreHealth);

        //4.4 플레이어/적 코어 체력 설정
        CoreSystem.Instance.SetUp(stageData);

        //5.플레이어 유닛 카드 덱 구성 (유닛 배치는 전투 중 카드로 진행)
        CardSystem.Instance.SetUp();

        //5.5 유저 소유 유물 구독 시작
        PerkSystem.Instance.SetUp();

        //6.전투 시작(Event)
        TurnGA turnGA = new(TurnType.StartBattle);
        ActionSystem.Instance.Perform(turnGA);

        yield return null;
    }
}
