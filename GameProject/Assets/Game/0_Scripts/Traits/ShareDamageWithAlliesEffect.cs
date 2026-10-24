using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//자신의 공격력을 divisor로 나눈 몫(내림)만큼 owner를 제외한 같은 진영 전원의 공격력을 영구 증가 (원작 확인 필요: 절반 반올림 규칙)
//KillGA(owner 사망) 구독을 가정, 권장 조건: SelfDeathCondition (reactionTiming PRE — owner가 아직 살아 있어 BaseDamage 조회 가능)
public class ShareDamageWithAlliesEffect : TraitEffect
{
    [SerializeField] private int divisor = 2;

    public override void PerformReaction(GameAction action, CombatantView owner)
    {
        int amount = owner.BaseDamage / divisor;
        if (amount <= 0) return;

        List<CombatantView> targets = owner is HeroView
            ? TokenSystem.Instance.HeroViews.Cast<CombatantView>().ToList()
            : TokenSystem.Instance.EnemyViews.Cast<CombatantView>().ToList();
        targets.Remove(owner);
        if (targets.Count == 0) return;

        ActionSystem.Instance.AddReaction(new ModifyStatGA(StatType.DAMAGE, amount, targets, owner));
    }
}
