using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IsoTools;

public class HeroView : CombatantView
{
    public void SetUp(HeroData heroData)
    {
        IsoObject isObject = GetComponent<IsoObject>();
        if (isObject == null)
            isObject = gameObject.AddComponent<IsoObject>();

        SetUpBase(heroData.Health, heroData, isObject);
    }
}
