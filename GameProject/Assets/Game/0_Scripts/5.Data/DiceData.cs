using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Dice")]
public class DiceData : ScriptableObject
{
    [field : SerializeField] public string Description { get; private set; }
    [field: SerializeField] public DiceModel DiceModel { get; private set; }
    [field : SerializeField] public int[] NumberPerFace { get; private set; }
}
