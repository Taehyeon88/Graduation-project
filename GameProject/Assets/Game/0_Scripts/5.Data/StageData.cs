using SerializeReferenceEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Stage")]
public class StageData : ScriptableObject
{
    [field: SerializeField] public WaveData waveData { get; private set; }
    [field: SerializeField] public Vector2Int[] ObstaclePositions { get; private set; }   //장애물 위치 (기본 배치 칸에 있으면 그 라인의 배치 위치가 한 칸 앞으로 이동)
    [field: SerializeField] public int PlayerCoreHealth { get; private set; } = 10;
    [field: SerializeField] public int EnemyCoreHealth { get; private set; } = 10;
    [field: SerializeField] public int StageBGMId { get; private set; }
}
