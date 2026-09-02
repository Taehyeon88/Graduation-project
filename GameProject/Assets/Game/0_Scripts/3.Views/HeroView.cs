using IsoTools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HeroView : CombatantView
{
    public int Id => TokenData.Id;
    public IReadOnlyList<Skill> Skills => Hero.Skills;    //보유 중인 스킬들
    public IReadOnlyList<PerkItem> Perks => Hero.Perks;    //보유 중인 아이템들

    public Hero Hero { get; private set; }

    public void SetUp(HeroData heroData)
    {
        IsoObject isObject = GetComponent<IsoObject>();
        if (isObject == null)
            isObject = gameObject.AddComponent<IsoObject>();

        SetUpBase(heroData.Hero.HeroHp,
            heroData.Hero.HeroMaxHp, 
            heroData.Hero.MovePoint, 
            heroData, 
            isObject
            );

        this.Hero = heroData.Hero;

        //GameSystem에서 Skill과 아이템들 불러오기
        //불러온 아이템들 효과들 적용
        foreach (var perk in Perks)
        {
            perk.SetOwner(this);
            perk.OnAdd();
        }
    }

    private void OnDisable()
    {
        foreach (var perk in Perks)
        {
            perk.OnRemove();
        }
    }
}
