using UnityEngine;

[CreateAssetMenu(menuName = "Data/PowerTotem")]
public class PowerTotemData : TokenData
{
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public Sprite Sprite { get; private set; }
    [field: SerializeField] public int Health { get; private set; }
    [field: SerializeField] public int Distance { get; private set; }
}
