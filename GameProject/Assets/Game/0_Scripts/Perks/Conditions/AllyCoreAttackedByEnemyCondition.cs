using System;

//적 용병이 아군 코어(용병대장)를 공격할 때 (Caster 없음 = 플레이어 스킬, HeroView = 아군 영웅이라 제외)
public class AllyCoreAttackedByEnemyCondition : PerkCondition
{
    public override void SubscribeCondition(Action<GameAction> reaction)
    {
        ActionSystem.SubscribeReaction<AttackCoreGA>(reaction, reactionTiming);
    }

    public override void UnsubscribeCondition(Action<GameAction> reaction)
    {
        ActionSystem.UnsubscribeReaction<AttackCoreGA>(reaction, reactionTiming);
    }

    public override bool SubConditionIsMat(GameAction action)
    {
        var attackCoreGA = action as AttackCoreGA;
        return attackCoreGA.Caster is EnemyView;
    }
}
