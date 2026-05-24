using IsoTools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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
