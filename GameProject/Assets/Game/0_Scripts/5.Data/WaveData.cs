using UnityEngine;

[CreateAssetMenu(menuName = "Data/Wave")]
public class WaveData : ScriptableObject
{
    [field: SerializeField] public int Id { get; private set; }
    [field: SerializeField] public EnemyData[] EnemyDatas { get; private set; }    //몬스터 리스트
    [field: SerializeField] public int[] WavePerEnemyCount { get; private set; }   //웨이브 당 생성될 몬스터 수
    [field: SerializeField] public int[] WaveTurnIntervals { get; private set; }   //다음 웨이브까지 대기할 몬스터 턴 수 (index 0 미사용)
    [field: SerializeField] public Vector2Int[] SpawnPositions { get; private set; } //몬스터별 생성 위치 (EnemyDatas와 같은 인덱스, 테스트용)
}
