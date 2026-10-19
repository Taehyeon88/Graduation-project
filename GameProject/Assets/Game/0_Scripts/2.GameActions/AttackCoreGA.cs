public class AttackCoreGA : GameAction, IHaveCaster
{
    public Token Caster { get; private set; }
    public int? Amount { get; private set; }   //피해량 (null이면 Caster.BaseDamage)
    public bool IsRangedAttack { get; private set; }   //공격 모션을 근접 돌진 대신 화살 발사로 재생

    public AttackCoreGA(Token caster)
    {
        Caster = caster;
    }

    public AttackCoreGA(Token caster, int amount)
    {
        Caster = caster;
        Amount = amount;
    }

    public AttackCoreGA(Token caster, bool isRangedAttack)
    {
        Caster = caster;
        IsRangedAttack = isRangedAttack;
    }
}
