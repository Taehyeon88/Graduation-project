using UnityEngine;

[CreateAssetMenu(menuName = "Data/Token/Hero")]
public class HeroData : CombatantData
{
    [field: SerializeField] public Color heroColor { get; private set; }
    [field: SerializeField] public Sprite simbolIcon { get; private set; }
    [field: SerializeField] public int APCost { get; private set; }   //카드로 배치할 때 드는 AP
}
