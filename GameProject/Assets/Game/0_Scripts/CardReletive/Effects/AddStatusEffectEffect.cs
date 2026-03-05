using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddStatusEffectEffect : Effect
{
    [SerializeField] private StatusEffectType statusEffectType;
    [SerializeField] private int stackCount;
    public override GameAction GetGameAction(EffectInfo effectInfo)
    {
        return new AddStatusEffectGA(statusEffectType, stackCount, effectInfo.targets);
    }
}
