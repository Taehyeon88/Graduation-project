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
    [field: SerializeField] public bool UsePrivateLogic { get; private set; } = false;     //어깨치기와 같이 EffecSystem에서 사용되는 것이 아닌 VisualEffect

}
