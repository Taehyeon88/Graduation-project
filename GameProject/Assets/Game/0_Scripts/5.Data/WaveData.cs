using UnityEngine;

[CreateAssetMenu(menuName = "Data/Wave")]
public class WaveData : ScriptableObject
{
    [field: SerializeField] public int Id { get; private set; }
    [field: SerializeField] public EnemyData[] EnemyDatas { get; private set; }    //몬스터 리스트
    [field: SerializeField] public int[] WavePerEnemyCount { get; private set; }   //웨이브 당 생성될 몬스터 수
    [field: SerializeField] public WaveCoreData WaveCoreData { get; private set; } //웨이브 핵 데이터
}
