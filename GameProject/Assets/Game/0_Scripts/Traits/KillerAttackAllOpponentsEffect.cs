using System.Collections.Generic;
using System.Linq;

//소유자(장애물)의 공격력으로 처치자의 상대 진영 모든 용병을 추가 공격 (처치자의 힘·치명타 등 보정 없음, KillGA를 PRE로 받는 조건과 함께 사용)
//처치자는 어느 진영을 칠지 정하는 데만 쓰고, 피해량과 Caster는 소유자다
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
            ActionSystem.Instance.AddReaction(new DealDamageGA(owner.BaseDamage, targets, owner));
    }
}
