using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddStatusEffectEffect : Effect
{
    [SerializeField] private StatusEffectType statusEffectType;
    [SerializeField] private int stackCount;
    [SerializeField] public SETargetMode etargetMode = SETargetMode.MySelf;    //(이펙트 효과 받는 대상 = 나) 여부 체크
    public override GameAction GetGameAction(EffectInfo effectInfo)
    {
        List<CombatantView> targets = new();
        switch (etargetMode)
        {
            case SETargetMode.MySelf: targets.Add(effectInfo.caster); break;
            case SETargetMode.Targets: targets.AddRange(effectInfo.targets); break;
            case SETargetMode.Entire:
                targets.AddRange(effectInfo.targets);
                targets.Add(effectInfo.caster);
                break;
        }
        return new AddStatusEffectGA(statusEffectType, stackCount, targets);
    }
}