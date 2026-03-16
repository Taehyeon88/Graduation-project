using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct StatusEffectInfo
{
    //¹æ¾î¸·N

    //È¥¶õN
    public float disarrayPercent;

    public void SetStatusEffectInfo(float[] infoes, StatusEffectType type)
    {
        switch (type)
        {
            case StatusEffectType.ARMOR: break;

            case StatusEffectType.DISARRAY:
                disarrayPercent = infoes[0];
            break;


            default: break;
        }
    }
}
