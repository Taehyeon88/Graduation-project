using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerformEffectGA : GameAction
{
    public Effect Effect { get; set; }
    public List<Vector2Int> TargetPoses { get; private set; }
    public HeroView MyView {  get; private set; }
    public PerformEffectGA(Effect effect, List<Vector2Int> targetpoes, HeroView myView)
    {
        Effect = effect;
        TargetPoses = targetpoes;
        MyView = myView;
    }
}
