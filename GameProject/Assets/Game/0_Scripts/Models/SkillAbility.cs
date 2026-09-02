using System.Collections;
using System.Collections.Generic;
using SerializeReferenceEditor;
using UnityEngine;

[System.Serializable]
public class SkillAbility
{
    [field: SerializeField] public List<TargetType> TargetTypes { get; private set; }
    [field: SerializeReference, SR] public TargetMode TargetMode { get; private set; }
    [field: SerializeReference, SR] public RangeMode RangeMode { get; private set; }
    [field: SerializeField] public int Distance { get; private set; } = 1;

    [field: SerializeField] public string TargetVG { get; private set; }
    [field: SerializeField] public string RangeVG { get; private set; }

    [field: SerializeReference, SR] public List<Effect> Effects { get; private set; }

    [Header("옵션")]
    [field: SerializeField] public bool IsSelfPossitive { get; private set; }
    [field: SerializeField] public bool IsFriendlyPossitive { get; private set; }

}