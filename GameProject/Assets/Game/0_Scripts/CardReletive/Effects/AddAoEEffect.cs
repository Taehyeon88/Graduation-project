using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddAoEEffect : Effect
{
    [SerializeField] private AoEType aoEType;
    public override GameAction GetGameAction(EffectInfo effectInfo)
    {
        return new AddAoEGA(effectInfo.caster, aoEType, effectInfo.targetPoses);
    }
}
