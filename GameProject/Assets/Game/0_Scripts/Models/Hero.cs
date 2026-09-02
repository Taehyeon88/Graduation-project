using System.Collections.Generic;
using UnityEngine;

public class Hero
{
    public int Id { get; private set; }
    public int HeroHp { get; private set; }
    public int HeroMaxHp { get; private set; }
    public int MovePoint { get; private set; }
    public List<Skill> Skills { get; private set; } = new(10);  //보유 중인 스킬
    public List<PerkItem> Perks { get; private set; } = new(10); //보유 중인 아이템들

    public Hero(int id, int heroHP, int movePoint, List<Skill> skills, List<PerkItem> perks)
    {
        Id = id;
        HeroHp = HeroMaxHp = heroHP;
        MovePoint = movePoint;
        Skills = skills;
        Perks = perks;
    }

    //나중에 최대 체력 증가 or 체력 변경 시, 별도로 함수 선언
}
