using System;

//적이 소유자(용병대장)의 코어를 공격한 뒤 (공격자가 Caster이므로 Caster 필터를 끈다)
public class CoreAttackedCondition : TraitCondition
{
    public override bool FiltersByCaster => false;

    public override void SubscribeCondition(Action<GameAction> reaction)
    {
        ActionSystem.SubscribeReaction<AttackCoreGA>(reaction, reactionTiming);
    }

    public override void UnsubscribeCondition(Action<GameAction> reaction)
    {
        ActionSystem.UnsubscribeReaction<AttackCoreGA>(reaction, reactionTiming);
    }

    public override bool SubConditionIsMat(GameAction action, CombatantView owner)
    {
        var attackCoreGA = action as AttackCoreGA;
        return owner is HeroCaptainView && owner.CurrentHealth > 0 && !attackCoreGA.IsMissed && attackCoreGA.Caster != null && attackCoreGA.Caster is not HeroView;   //Caster 없음 = 플레이어 스킬의 적 코어 공격, IsMissed = 피해 없이 빗나간 공격
    }
}
