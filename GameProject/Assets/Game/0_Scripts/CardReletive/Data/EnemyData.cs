using System.Collections;
using System.Collections.Generic;
using SerializeReferenceEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Enemy")]
public class EnemyData : TokenData
{
    [field : SerializeField]public int Health { get; private set; }
    [field : SerializeField]public int AttackPower { get; private set; }
    [field : SerializeReference, SR]public Enemy Enemy { get; private set; }
}
