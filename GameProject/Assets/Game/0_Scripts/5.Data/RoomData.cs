using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/RoomData")]
public class RoomData : ScriptableObject
{
    [field: SerializeField] public int Stage { get; private set; }
    [field: SerializeField] public bool IsBossRoom { get; private set; }
    [field: SerializeField] public Vector2Int[] HeroSetUpPositions { get; private set; }
    [field: SerializeField] public WaveData WaveData { get; private set; }
    [field: SerializeField] public MapThemeData MapThemeData { get; private set; }

    [field: Header("오브젝트 커스텀 설정")]
    [field: SerializeField] public bool Custom_Set_Obj { get; private set; } = false;
    [field: SerializeField] public List<TokenData> ObstacleDatas { get; private set; }
    [field: SerializeField] public List<Vector2Int> ObstacleSetUpPositions { get; private set; }
}
