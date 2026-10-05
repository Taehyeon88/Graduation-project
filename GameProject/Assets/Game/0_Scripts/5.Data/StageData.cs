using SerializeReferenceEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Stage")]
public class StageData : ScriptableObject
{
    [field: SerializeField] public WaveData waveData { get; private set; }
    [field: SerializeField] public Vector2Int[] HeroSetupPoses { get; private set; }
    [field: SerializeField] public int StageBGMId { get; private set; }
}
