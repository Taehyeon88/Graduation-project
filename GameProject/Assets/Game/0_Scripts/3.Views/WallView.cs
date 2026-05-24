using System.Collections;
using System.Collections.Generic;
using IsoTools;
using UnityEngine;

public class WallView : Token
{
    public void SetUp(WallData wallData)
    {
        IsoObject isObject = GetComponent<IsoObject>();
        if (isObject == null)
            isObject = gameObject.AddComponent<IsoObject>();

        SetUpBaseBase(wallData, isObject);
    }
}
