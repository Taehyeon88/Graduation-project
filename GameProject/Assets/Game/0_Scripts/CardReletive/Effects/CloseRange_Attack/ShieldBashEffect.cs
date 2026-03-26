using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldBashEffect : Effect
{
    [SerializeField] private int amount;
    public override GameAction GetGameAction(EffectInfo effectInfo)
    {
        var shieldBashGA = new ShieldBashGA(CalculateDamage(amount), effectInfo.targetPoses);
        InitDamageRate();
        return shieldBashGA;
    }
}
