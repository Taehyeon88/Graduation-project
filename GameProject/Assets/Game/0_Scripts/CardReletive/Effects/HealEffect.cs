using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealEffect : Effect
{
    [SerializeField] private float amount;
    public override GameAction GetGameAction(EffectInfo effectInfo)
    {
        if (effectInfo.targets != null)           //대상 기반
        {
            return new HealGA(amount, effectInfo.targets);
        }
        else if(effectInfo.targetPoses != null)   //그리드 기반
        {
            return new HealGA(amount, effectInfo.targetPoses);
        }
        else
        {
            Debug.LogError($"HealEffect의 effectInfo에 target도 targetPoses도 존재하지 않습니다.");
            return null;
        }
    }
}
