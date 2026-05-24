using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AddStatusEffectEffect : Effect, IUseCondition
{
    [SerializeField] private StatusEffectType statusEffectType;
    [SerializeField] private int stackCount;
    [SerializeField] public SETargetMode setargetMode = SETargetMode.MySelf;    //(이펙트 효과 받는 대상 = 나) 여부 체크
    public override GameAction GetGameAction(EffectInfo effectInfo)
    {
        List<CombatantView> targets = new();
        switch (setargetMode)
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

    public bool[] IsMeetCondition(List<Vector2Int> targetPoses)
    {
        if (statusEffectType == StatusEffectType.DETERIORATE)
        {
            bool[] result = new bool[targetPoses.Count];

            for (int i = 0; i < targetPoses.Count; i++)
            {
                var targetPos = targetPoses[i];
                CombatantView target = TokenSystem.Instance.GetTokenByPosition(targetPos) as CombatantView;
                if (target != null)
                {
                    int poisionStatck = target.GetStatusEffectStacks(StatusEffectType.POISIONING);
                    int bleedingStatck = target.GetStatusEffectStacks(StatusEffectType.BLEEDING);
                    if (poisionStatck > 0 || bleedingStatck > 0)
                    {
                        result[i] = true;
                    }
                }
            }
            return result;
        }
        return null;
    }
}