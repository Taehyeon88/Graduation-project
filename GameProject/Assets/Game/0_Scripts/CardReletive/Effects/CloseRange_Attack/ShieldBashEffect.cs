using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldBashEffect : Effect
{
    [SerializeField] private float amount;
    public override GameAction GetGameAction(EffectInfo effectInfo)
    {
        var shieldBashGA = new ShieldBashGA(amount, effectInfo.targetPoses);
        return shieldBashGA;
    }
}
