using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddStatusEffectEffect : Effect
{
    [SerializeField] private StatusEffectType statusEffectType;
    [SerializeField] private int stackCount;
    [SerializeField] public bool isMySelf = true;    //(이펙트 효과 받는 대상 = 나) 여부 체크
    public override GameAction GetGameAction(EffectInfo effectInfo)
    {
        if (isMySelf)
            return new AddStatusEffectGA(statusEffectType, stackCount, new(){effectInfo.caster});
        else
            return new AddStatusEffectGA(statusEffectType, stackCount, effectInfo.targets);
    }
}
