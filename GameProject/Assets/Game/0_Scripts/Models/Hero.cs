using System.Collections.Generic;
using UnityEngine;

public class Hero
{
    public const int MaxPerkCount = 2;   //용병당 특성 최대 개수

    public int Id { get; private set; }
    public int HeroHp { get; private set; }
    public int HeroMaxHp { get; private set; }
    public List<Skill> Skills { get; private set; } = new(10);  //보유 중인 스킬
    public List<PerkItem> Perks { get; private set; } = new(10); //보유 중인 아이템들

    public Hero(int id, int heroHP, List<Skill> skills, List<PerkItem> perks)
    {
        Id = id;
        HeroHp = HeroMaxHp = heroHP;
        Skills = skills;

        if (perks.Count > MaxPerkCount)
        {
            Debug.LogError($"Hero({id}): 특성은 최대 {MaxPerkCount}개까지 가능합니다. (현재 {perks.Count}개) 앞의 {MaxPerkCount}개만 사용합니다.");
            perks = perks.GetRange(0, MaxPerkCount);
        }
        Perks = perks;
    }

    //나중에 최대 체력 증가 or 체력 변경 시, 별도로 함수 선언
}
