using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Data/Map/Wave")]
public class WaveData : ScriptableObject
{
    [field: SerializeField] public EnemyData[] EnemyDatas { get; private set; }
    [field: SerializeField] public List<int> EnemyCountsPerWave { get; private set; }
}
