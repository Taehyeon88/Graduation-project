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
    [field: SerializeReference, SR] public List<Effect> Effects { get; private set; }
}