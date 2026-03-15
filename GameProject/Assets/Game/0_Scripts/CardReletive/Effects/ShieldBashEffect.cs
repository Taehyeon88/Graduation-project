using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldBashEffect : Effect
{
    [SerializeField] private int amount;
    public override GameAction GetGameAction(EffectInfo effectInfo)
    {
        return new ShieldBashGA(amount, effectInfo.targetPoses);
    }
}
