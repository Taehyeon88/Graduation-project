using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectStorage
{
    //방어막N

    //혼란N
    public float disarrayPercent { get; private set; }
    //표적N
    public float markPercent { get; private set; }

    public void SetStatusEffectInfo(float[] infoes, StatusEffectType type)
    {
        switch (type)
        {
            case StatusEffectType.ARMOR: break;

            case StatusEffectType.DISARRAY:
                disarrayPercent = infoes[0];
            break;

            case StatusEffectType.MARK:
                markPercent = infoes[0];
            break;


            default: break;
        }
    }
}
