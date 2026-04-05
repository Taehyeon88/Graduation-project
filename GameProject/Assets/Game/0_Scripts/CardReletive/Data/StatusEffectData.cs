using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/StatusEffectData")]
public class StatusEffectData : ScriptableObject
{
    [field: SerializeField] public StatusEffectType EffectType { get; private set; }
    [field: SerializeField] public SEMachanicsType SEMachanicsType { get; private set; }
    [field: SerializeField] public string Discription { get; private set; }
    [field: SerializeField] public Sprite spriteImage { get; private set; }
    [field: SerializeField] public float[] effectInfos { get; private set; }
}
