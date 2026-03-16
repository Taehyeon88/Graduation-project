using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExecutionEffect : Effect
{
    [SerializeField] private int amount;
    public override GameAction GetGameAction(EffectInfo effectInfo)
    {
        return new ExecutionGA(amount, effectInfo.targetPoses);
    }
}
