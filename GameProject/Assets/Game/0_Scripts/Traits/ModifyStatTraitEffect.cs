using System;
using System.Collections.Generic;
using UnityEngine;

//소유자 자신의 스탯들을 각각 amount만큼 증가
public class ModifyStatTraitEffect : TraitEffect, ILimitedTrigger
{
    [Serializable]
    private struct StatModifier
    {
        public StatType statType;
        public int amount;
    }

    [SerializeField] private List<StatModifier> modifiers = new();
    [SerializeField] private int maxTriggers;   //유닛당 발동 횟수 상한 (0 = 무제한)

    public int MaxTriggers => maxTriggers;

    public override void PerformReaction(GameAction action, CombatantView owner)
    {
        foreach (var modifier in modifiers)
        {
            ModifyStatGA modifyStatGA = new(modifier.statType, modifier.amount, new() { owner }, owner);
            ActionSystem.Instance.AddReaction(modifyStatGA);
        }
    }
}
