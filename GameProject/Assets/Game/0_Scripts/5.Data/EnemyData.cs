using System.Collections;
using System.Collections.Generic;
using SerializeReferenceEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Token/Enemy")]
public class EnemyData : TokenData
{
    [field : SerializeReference, SR]public Enemy Enemy { get; private set; }
    [field : SerializeReference, SR]public List<EnemyAction> EnemyActions { get; private set; }
}
