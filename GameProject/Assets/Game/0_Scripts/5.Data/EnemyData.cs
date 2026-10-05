using UnityEngine;

[CreateAssetMenu(menuName = "Data/Token/Enemy")]
public class EnemyData : TokenData
{
    [field: SerializeField] public int Health { get; private set; }
    [field: SerializeField] public int MovePoint { get; private set; }
}
