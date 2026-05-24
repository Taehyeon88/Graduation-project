using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/RoomData")]
public class RoomData : ScriptableObject
{
    [field: SerializeField] public int Stage { get; private set; }
    [field: SerializeField] public int Level {  get; private set; }
    [field: SerializeField] public List<EnemyData> enemyDatas {  get; private set; }
    [field: SerializeField] public List<WallData> wallDatas   {  get; private set; }
    [field: SerializeField] public List<Vector2Int> heroSetUpPositions {  get; private set; }
    [field: SerializeField] public List<Vector2Int> enemySetUpPositions {  get; private set; }
    [field: SerializeField] public List<Vector2Int> wallSetUpPositions {  get; private set; }
}
