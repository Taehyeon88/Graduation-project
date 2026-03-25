using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectStorage
{
    //info가 필요 없는 것: 방어막N, 고립N

    //혼란N
    public float disarrayPercent { get; private set; }
    //표적N
    public float markPercent { get; private set; }

    public void SetStatusEffectInfo(float[] infoes, StatusEffectType type)
    {
        switch (type)
        {
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
