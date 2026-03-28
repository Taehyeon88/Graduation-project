using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AoE
{
    public AoEModel AoEModel => aoEData.AoEModel;                                //장판 모델(스프라이트 포함)
    public AoEFieldType AoEFieldType => aoEData.AoEFieldType;                    //장판 계열(오브젝트 or 필드)
    public AoEType AoEType => aoEData.AoEType;                                   //장판 타입
    public List<Effect> statusEffects => aoEData.statusEffects;                  //대상 지정 부여하는 상태효과
    public bool UseCountBased => aoEData.UseCountBased;                          //횟수 기반 여부(T - 횟수 | F - 턴)
    public int EntryDamage { get; private set; }                                 //진입 피해
    public int TurnDamage { get; private set; }                                  //턴당 피해
    public int MaxDuration { get; private set; }                                 //최대 지속 턴 수 혹은 횟수
    public int RemainDuration { get; private set; }                              //남은 지속 턴 수 혹은 횟수
    public CombatantView Caster { get; private set; }

    private readonly AoEData aoEData;


    public AoE(AoEData aoEData, CombatantView caster)
    {
        this.aoEData = aoEData;
        this.Caster = caster;
        EntryDamage = aoEData.EntryDamage;
        TurnDamage = aoEData.TurnDamage;
        RemainDuration = MaxDuration = aoEData.UseCountBased? 
            aoEData.DurationCount : aoEData.DurationTurn;
    }

    public int ReduceRemainDuration(int amount = 1)
    {
        RemainDuration -= amount;
        return RemainDuration;
    }
}
