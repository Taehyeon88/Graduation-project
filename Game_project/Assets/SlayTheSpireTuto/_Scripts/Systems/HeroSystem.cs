using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroSystem : Singleton<HeroSystem>
{
    [field : SerializeField] public HeroView HeroView { get; private set; }

    public void SetUp(HeroData heroData)
    {
        HeroView.SetUp(heroData);
    }
}
