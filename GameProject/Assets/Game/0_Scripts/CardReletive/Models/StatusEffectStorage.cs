using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectStorage
{
    //info가 필요 없는 것: 방어막N, 고립N

    //혼란N
    public float Disarray_Percent { get; private set; }
    //표적N
    public float Mark_Percent { get; private set; }
    //집중N
    public float Concentration_Percent { get; private set; }
    //독물N
    public float Poision_Percent { get; private set; }
    //출혈N
    public float Bleeding_Percent { get; private set; }
    //악화N
    public float Deteriorate_Rate { get; private set; }

    public void SetStatusEffectInfo(float[] infoes, StatusEffectType type)
    {
        switch (type)
        {
            case StatusEffectType.DISARRAY:
                Disarray_Percent = infoes[0];
            break;

            case StatusEffectType.MARK:
                Mark_Percent = infoes[0];
            break;

            case StatusEffectType.CONCENTRATION:
                Concentration_Percent = infoes[0];
            break;

            case StatusEffectType.POISIONING:
                Poision_Percent = infoes[0];
                break;

            case StatusEffectType.BLEEDING:
                Bleeding_Percent = infoes[0];
                break;

            case StatusEffectType.DETERIORATE:
                Deteriorate_Rate = infoes[0];
                break;

            default: break;
        }
    }
}
