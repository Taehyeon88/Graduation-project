public class AttackCoreGA : GameAction, IHaveCaster
{
    public Token Caster { get; private set; }

    public AttackCoreGA(Token caster)
    {
        Caster = caster;
    }
}
