using System.Collections;
using System.Collections.Generic;
using SerializeReferenceEditor;
using UnityEngine;

[System.Serializable]
public class GridTargetMode
{
    [field: SerializeField] public int Distance { get; private set; } = 1;
    [field: SerializeReference, SR] public TargetMode TargetMode { get; private set; }
    [field: SerializeReference, SR] public GridRangeMode GridRangeMode { get; private set; }

    public bool UseVisualGrid;
    public bool UseSelectVG;

    [ShowIf("UseVisualGrid")]
    public string WillSelectVGName;

    [ShowIf("UseSelectVG")]
    public string SelectVGName;

    [field: SerializeField] public EffectTargetCondition EffectCondition { get; private set; } = EffectTargetCondition.Grid;
    [field: SerializeReference, SR] public Effect Effect { get; private set; }

    [field: Space(7)]
    [field: Tooltip("UseVisualGrid가 True일 때만 사용가능!")]
    [field: SerializeField] public EffectTargetCondition _AddedEffectCondition { get; private set; } = EffectTargetCondition.CombatantView;

    [field: Tooltip("UseVisualGrid가 True일 때만 사용가능!")]
    [field: SerializeReference, SR] public List<Effect> AddedEffects { get; private set;}


    public enum EffectTargetCondition
    {
        Grid,
        CombatantView
    }
}