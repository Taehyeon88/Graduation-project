using System;
using System.Collections.Generic;
using UnityEngine;

//소유자에게 피해를 준 공격자(DealDamageGA.Caster)의 스탯을 각각 amount만큼 증가 (공격자가 CombatantView가 아니면 무시)
//구독 GA는 DealDamageGA여야 한다. 권장 조건: SelfDamagedCondition (POST)
public class ModifyStatAttackerEffect : TraitEffect
{
    [Serializable]
    private struct StatModifier
    {
        public StatType statType;
        public int amount;
    }

    [SerializeField] private List<StatModifier> modifiers = new();

    public override void PerformReaction(GameAction action, CombatantView owner)
    {
        var dealDamageGA = action as DealDamageGA;
        if (dealDamageGA.Caster is not CombatantView attacker) return;

        foreach (var modifier in modifiers)
            ActionSystem.Instance.AddReaction(new ModifyStatGA(modifier.statType, modifier.amount, new() { attacker }, owner));
    }
}
