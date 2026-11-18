using SerializeReferenceEditor;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Data/Stage")]
public class StageData : ScriptableObject
{
    [field: SerializeField] public WaveData waveData { get; private set; }
    [field: SerializeField, FormerlySerializedAs("<ObstaclePositions>k__BackingField")] public Vector2Int[] NeutralPositions { get; private set; }   //중립지대 위치 (이동·배치 불가, 기본 배치 칸에 있으면 그 라인의 배치 위치가 한 칸 앞으로 이동)
    [field: SerializeField] public ObstacleSpawn[] ObstacleSpawns { get; private set; }   //파괴 가능 장애물 배치
    [field: SerializeField] public TerrainSpawn[] TerrainSpawns { get; private set; }   //발판(풀숲) 배치
    [field: SerializeField] public int EnemyCoreHealth { get; private set; } = 10;
    [field: SerializeField] public int StageBGMId { get; private set; }

    [Header("스테이지 선택 (덱·보상)")]
    [field: SerializeField] public int HeroCaptainHealth { get; private set; } = 20;   //용병대장(플레이어) 체력 — 스테이지마다 이 값으로 시작
    [field: SerializeField] public int DeckSize { get; private set; } = 5;             //시작 덱 크기(5~8). 선택 슬롯 = DeckSize - BaseHeroes 수
    [field: SerializeField] public HeroData[] BaseHeroes { get; private set; }         //기본 제공 영웅 카드 (고정, 제외 불가)
    [field: SerializeField] public int InsigniaReward { get; private set; } = 1;       //첫 클리어 휘장 수량 (지급량은 기획 보류 — 임시값)

    [Header("튜토리얼 (비우면 일반 스테이지)")]
    [field: SerializeField] public TutorialBattleData TutorialBattle { get; private set; }

    [Header("별 조건 (★1 승리는 고정)")]
    [field: SerializeField] public int CaptainHealthStarThreshold { get; private set; } = 10;   //★2: 용병대장 체력 N 이상 남기고 승리
    [field: SerializeReference, SR] public StarCondition SpecialStarCondition { get; private set; }   //★3: 스테이지 특수 조건
}
