using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySkillGA : GameAction, IHaveCaster
{
    public Skill Skill { get; set; }
    public List<Vector2Int> TargetPoses { get; private set; }
    public HeroView Caster { get; private set; }
    Token IHaveCaster.Caster => Caster;
    public PlaySkillGA(Skill skill, List<Vector2Int> targetPoses, HeroView myView)
    {
        Skill = skill;
        TargetPoses = targetPoses;
        Caster = myView;
    }
}
