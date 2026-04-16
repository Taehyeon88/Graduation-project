using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/VisualEffectData")]
public class VisualEffectData : ScriptableObject
{
    [field: SerializeField] public CardType CardType { get; private set; }
    [field: SerializeField] public CardSubType CardSubType { get; private set; }
    [field: SerializeField] public List<CustomSquenceAndSound> CustomSquences { get; private set; }
    [field: SerializeField] public GameObject HitVFX { get; private set; }
    [field: SerializeField] public int HitSoundId { get; private set; }

}
