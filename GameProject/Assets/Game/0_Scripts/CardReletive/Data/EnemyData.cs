using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Enemy")]
public class EnemyData : TokenData
{
    [field : SerializeField]public int Health { get; private set; }
    [field : SerializeField]public int AttackPower { get; private set; }

}
