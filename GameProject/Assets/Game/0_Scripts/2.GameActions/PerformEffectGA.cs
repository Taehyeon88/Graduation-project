using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerformEffectGA : GameAction, IHaveCaster
{
    public Effect Effect { get; set; }
    public List<Vector2Int> TargetPoses { get; private set; }
    public HeroView Caster { get; private set; }
    Token IHaveCaster.Caster => Caster;
    public PerformEffectGA(Effect effect, List<Vector2Int> targetpoes, HeroView myView)
    {
        Effect = effect;
        TargetPoses = targetpoes;
        Caster = myView;
    }
}
