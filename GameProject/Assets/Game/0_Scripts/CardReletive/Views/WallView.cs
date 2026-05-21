using System.Collections;
using System.Collections.Generic;
using IsoTools;
using UnityEngine;

public class WallView : CombatantView
{
    public void SetUp(WallData wallData)
    {
        IsoObject isObject = GetComponent<IsoObject>();
        if (isObject == null)
            isObject = gameObject.AddComponent<IsoObject>();

        SetUpBase(1000000, wallData, isObject);
    }

    public override void AddStatusEffect(StatusEffectType type, int stackCount, Sprite sprite, float[] infoes = null) { }

    public override void RemoveStatusEffect(StatusEffectType type, int stackCount) { }
}
