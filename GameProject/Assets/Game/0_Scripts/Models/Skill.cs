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
    public int Limit { get; private set; }
    public int MaxLimit { get; private set; }

    public readonly SkillData data;

    public Skill(SkillData data)
    {
        this.data = data;
        MaxLimit = Limit = data.Limit;

        string name = data.name;
        int index = name.IndexOf("_");
        Title = index >= 0? name.Substring(index + 1) : name;
    }

    public bool HasEnoughLimit()
    {
        return Limit > 0;
    }
    public void ReFillLimit()
    {
        Limit = MaxLimit;
    }
    public void ReduceLimit()
    {
        Limit--;
    }
}
