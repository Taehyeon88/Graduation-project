using UnityEngine;

//튜토리얼 전투의 고정 시나리오 (StageData에 연결된 스테이지만 튜토리얼로 동작)
[CreateAssetMenu(menuName = "Data/TutorialBattle")]
public class TutorialBattleData : ScriptableObject
{
    [Header("덱 고정")]
    [field: SerializeField] public HeroData[] DrawOrder { get; private set; }   //뽑을 카드 더미 순서 (맨 앞이 맨 위, 섞기 생략)
    [field: SerializeField] public int StartDrawAmount { get; private set; } = 1;   //전투 시작 드로우 장수 (첫 턴 3장 드로우 대체)

    [Header("치명타 고정")]
    [field: SerializeField] public int CriticalRound { get; private set; } = 1;   //이 라운드의 자동 전투 끝에만 고정
    [field: SerializeField] public HeroData[] CriticalHeroes { get; private set; }   //그 시점에 치명타 100%인 영웅 (그 밖의 영웅은 0%)

    [Header("같은 속도 주사위 고정")]
    [field: SerializeField] public int SameSpeedRound { get; private set; } = 1;   //이 라운드에만 고정
    [field: SerializeField] public CombatantData SameSpeedWinner { get; private set; }   //같은 속도 그룹에 있으면 먼저 행동하는 영웅 또는 몬스터
}
