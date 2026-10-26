using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//살아 있는 몬스터 중 targetEnemyId와 Id가 같은 전원의 스탯을 각각 amount만큼 증가
//권장 조건: TurnCondition (turnType Enemy, reactionTiming POST — 라운드 종료)
public class ModifyStatEnemyByIdEffect : TraitEffect
{
    [Serializable]
    private struct StatModifier
    {
        public StatType statType;
        public int amount;
    }

    [SerializeField] private int targetEnemyId;
    [SerializeField] private List<StatModifier> modifiers = new();

    public override void PerformReaction(GameAction action, CombatantView owner)
    {
        List<CombatantView> targets = TokenSystem.Instance.EnemyViews
            .Where(enemy => enemy.Id == targetEnemyId)
            .Cast<CombatantView>()
            .ToList();
        if (targets.Count == 0) return;

        foreach (var modifier in modifiers)
            ActionSystem.Instance.AddReaction(new ModifyStatGA(modifier.statType, modifier.amount, targets, owner));
    }
}
