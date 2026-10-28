using System.Collections.Generic;
using System.Linq;

//소유자를 처치한 용병이 상대 진영 모든 용병을 추가 공격 (KillGA를 PRE로 받는 조건과 함께 사용)
public class KillerAttackAllOpponentsEffect : TraitEffect
{
    public override void PerformReaction(GameAction action, CombatantView owner)
    {
        var killGA = action as KillGA;
        if (killGA.Killer is not CombatantView killer) return;

        List<IDamageable> targets = killer is HeroView
            ? TokenSystem.Instance.EnemyViews.Cast<IDamageable>().ToList()
            : TokenSystem.Instance.HeroViews.Cast<IDamageable>().ToList();

        if (targets.Count > 0)
            ActionSystem.Instance.AddReaction(new DealDamageGA(killer.BaseDamage, targets, killer));
    }
}
