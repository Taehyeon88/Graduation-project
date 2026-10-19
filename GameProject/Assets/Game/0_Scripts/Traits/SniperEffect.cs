using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//공격력을 발동마다 감소(하한 유지)하고, 자동 전투 행동을 무작위 적 공격으로 대체
//권장 조건: AutoBattleTurnCondition, reactionTiming POST (자동 전투의 모든 공격이 끝난 뒤 감소)
public class SniperEffect : TraitEffect, IBattleOverride
{
    [SerializeField] private int damageDecay = 1;   //발동당 공격력 감소량
    [SerializeField] private int minDamage = 1;     //공격력 하한

    public override void PerformReaction(GameAction action, CombatantView owner)
    {
        int amount = Mathf.Min(damageDecay, owner.BaseDamage - minDamage);
        if (amount <= 0) return;

        ActionSystem.Instance.AddReaction(new ModifyStatGA(StatType.DAMAGE, -amount, new() { owner }, owner));
    }

    //무작위 상대 유닛 공격, 상대가 없으면 상대 코어 공격
    public bool TryBattle(CombatantView owner)
    {
        List<CombatantView> opponents = owner is HeroView
            ? TokenSystem.Instance.EnemyViews.Cast<CombatantView>().ToList()
            : TokenSystem.Instance.HeroViews.Where(h => h != null).Cast<CombatantView>().ToList();

        if (opponents.Count > 0)
            ActionSystem.Instance.AddReaction(new DealDamageGA(owner.BaseDamage, opponents[Random.Range(0, opponents.Count)], owner, true, true));
        else
            ActionSystem.Instance.AddReaction(new AttackCoreGA(owner, true));

        return true;
    }
}
