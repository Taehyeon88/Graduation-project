using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySkillGA : GameAction
{
    public Skill Skill { get; set; }
    public List<Vector2Int> TargetPoses { get; private set; }
    public HeroView MyView {  get; private set; }
    public PlaySkillGA(Skill skill, List<Vector2Int> targetPoses, HeroView myView)
    {
        Skill = skill;
        TargetPoses = targetPoses;
        MyView = myView;
    }
}
