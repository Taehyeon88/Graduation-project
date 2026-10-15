using UnityEngine;

[CreateAssetMenu(menuName = "Data/Token/Hero")]
public class HeroData : CombatantData
{
    [field: SerializeField] public int APCost { get; private set; }   //카드로 배치할 때 드는 AP
}
