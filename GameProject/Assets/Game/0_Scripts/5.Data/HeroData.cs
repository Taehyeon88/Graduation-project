using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Token/Hero")]
public class HeroData : TokenData
{
    [field: SerializeField] public int HeroHp { get; private set; }
    [field: SerializeField] public int MovePoint { get; private set; }
    [field : SerializeField] public List<SkillData> Skills { get; private set; }
    [field : SerializeField] public int Gold { get; private set; }
    [field: SerializeField] public Color heroColor { get; private set; }
    [field: SerializeField] public Sprite simbolIcon { get; private set; }
    [field: SerializeField] public List<SkillData> StartingSkills { get; private set; }
    [field: SerializeField] public List<PerkData> StartingPerks { get; private set; }
    [field: SerializeField] public Hero Hero { get; private set; } //게임 내, 실 영웅 데이터

    public void SetHero(Hero hero)
    {
        Hero = hero;
    }
}
