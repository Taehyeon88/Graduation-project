using UnityEngine;

[CreateAssetMenu(menuName = "Data/Token/Hero")]
public class HeroData : CombatantData
{
    [field: SerializeField] public Color heroColor { get; private set; }
    [field: SerializeField] public Sprite simbolIcon { get; private set; }
}
