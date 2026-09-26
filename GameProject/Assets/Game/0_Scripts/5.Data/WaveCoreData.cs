using UnityEngine;

[CreateAssetMenu(menuName = "Data/WaveCore")]
public class WaveCoreData : TokenData
{
    [field: SerializeField] public int CoreHp { get; private set; }
    [field: SerializeField] public Vector2Int CorePosition { get; private set; }
}
