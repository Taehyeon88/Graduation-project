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
        SoundSystem.Instance.PlayBGM(stageData.StageBGMId);

        //2.스테이지 맵 생성
        TokenSystem.Instance.Setup.SetUpStageMap();
        //3.스테이지 기물 배치(잠깐 패스)

        //4.몬스터 배치
        TokenSystem.Instance.Setup.SetUPEnemys(stageData.Enemies.ToArray(), stageData.EnemyPoses.ToArray());

        //4.5 영웅 아이템 정보 UI 설정
        allPerksDisplayUI.SetUp(heroDatas.ToArray());

        //5.플레이어 유닛 배치
        TokenSystem.Instance.Setup.StartSetUpHero(heroDatas.ToArray(), stageData.HeroSetupPoses.ToList());
        yield return new WaitUntil(() => !Interactions.Instance.IsSetUpHero);
        HeroSystem.Instance.CurrentHero = TokenSystem.Instance.HeroViews[0];

        //6.전투 시작(Event)
        TurnGA turnGA = new(TurnType.StartBattle);
        ActionSystem.Instance.Perform(turnGA);

        yield return null;
    }
}
