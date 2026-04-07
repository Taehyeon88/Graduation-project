using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DealDamageEffect : Effect
{
    [SerializeField] private float amount;
    public override GameAction GetGameAction(EffectInfo effectInfo)
    {
        DealDamageGA dealDamageGA = new(amount, effectInfo.targets, effectInfo.caster);
        return dealDamageGA;
    }
}
