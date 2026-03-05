using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerformEffectGA : GameAction
{
    public Effect Effect { get; set; }
    public EffectInfo EffectInfo { get; set; }
    public PerformEffectGA(Effect effect, EffectInfo effectInfo)
    {
        Effect = effect;
        EffectInfo = effectInfo;
    }
}
