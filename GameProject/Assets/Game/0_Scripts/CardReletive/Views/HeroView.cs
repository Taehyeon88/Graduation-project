using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroView : CombatantView
{
    public void SetUp(HeroData heroData)
    {
        SetUpBase(heroData.Health, heroData.TokenModel);
    }
}
