using UnityEngine;

[CreateAssetMenu(menuName = "Data/Token/Obstacle")]
public class ObstacleData : CombatantData
{
    [field: SerializeField, TextArea] public string Description { get; private set; }   //장애물 설명
}
