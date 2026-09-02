using UnityEngine;

[CreateAssetMenu(menuName = "Data/Stage")]
public class StageData : ScriptableObject
{
    [field: SerializeField] public int Chapter { get; private set; }
    [field: SerializeField] public EnemyData[] Enemies { get; private set; }
    [field: SerializeField] public Vector2Int[] EnemyPoses { get; private set; }
    [field: SerializeField] public Vector2Int[] HeroSetupPoses { get; private set; }
    
    //기물 데이터 배열
    //기물 데이터 위치
    [field: SerializeField] public int R_weight { get; private set; }
    [field: SerializeField] public bool IsBoss { get; private set; }
    [field: SerializeField] public int StageBGMId { get; private set; }
}
