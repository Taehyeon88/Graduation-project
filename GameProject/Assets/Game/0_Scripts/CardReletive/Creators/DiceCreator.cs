using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceCreator : Singleton<DiceCreator>
{
    public Transform CreateDice(DiceData data, Vector3 setupPosition, Transform parent)
    {
        return Instantiate(data.DiceModel, setupPosition, Quaternion.identity, parent).transform;
    }
}
