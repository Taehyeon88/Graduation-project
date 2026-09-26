using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Skill
{
    public int Id => data.Id;
    public string Description => data.Description;
    public Sprite Image => data.Image;
    public SkillType CardType => data.SkillType;
    public SkillAbility SkillAbility => data.SkillAbility;
    public string Title { get; private set; }

    public readonly SkillData data;

    public Skill(SkillData data)
    {
        this.data = data;

        string name = data.name;
        int index = name.IndexOf("_");
        Title = index >= 0? name.Substring(index + 1) : name;
    }
}
