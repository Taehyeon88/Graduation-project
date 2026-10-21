//코어 공격을 빗나가게 함 (CoreSystem이 IsMissed를 보고 모션만 재생하고 피해는 건너뜀)
public class MissCoreAttackEffect : PerkEffect
{
    public override void PerformReaction(GameAction action)
    {
        var attackCoreGA = action as AttackCoreGA;
        attackCoreGA.IsMissed = true;
    }
}
