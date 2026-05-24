using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AddAoEEffect : Effect
{
    [SerializeField] private AoEType aoEType;
    [SerializeField] private AoETargetMode aoETargetMode;
    public override GameAction GetGameAction(EffectInfo effectInfo)
    {
        return new AddAoEGA(
            effectInfo.caster, 
            aoEType, 
            effectInfo.targetPoses,
            aoETargetMode
            );
    }
}
