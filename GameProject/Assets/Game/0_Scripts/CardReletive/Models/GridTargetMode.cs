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

    [ShowIf("UseVisualGrid")]
    public bool UseSelectVG;

    [ShowIf("UseVisualGrid")]
    public string WillSelectVGName;

    [ShowIf("UseSelectVG", "UseVisualGrid")]
    public string SelectVGName;

    [field: SerializeReference, SR] public Effect Effect { get; private set; }

    [field: Space(7)]
    [field: Tooltip("UseVisualGrid가 True일 때만 사용가능!")]
    [field: SerializeField] public AddedSECondition _AddedSECondition { get; private set; } = AddedSECondition.Grid;

    [field: Tooltip("UseVisualGrid가 True일 때만 사용가능!")]
    [field: SerializeReference, SR] public List<Effect> AddedEffects { get; private set;}


    public enum AddedSECondition
    {
        Grid,
        CombatantView
    }
}