using UnityEngine;

[CreateAssetMenu(menuName = "Data/Destructible")]
public class DestructibleData : TokenData
{
    [field: SerializeField] public int Health { get; private set; }
}
