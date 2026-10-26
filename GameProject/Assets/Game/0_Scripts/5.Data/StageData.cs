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
}
