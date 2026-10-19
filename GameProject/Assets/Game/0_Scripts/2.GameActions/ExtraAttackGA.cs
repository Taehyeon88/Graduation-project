public class ExtraAttackGA : GameAction, IHaveCaster
{
    public CombatantView Caster { get; private set; }
    Token IHaveCaster.Caster => Caster;

    public ExtraAttackGA(CombatantView caster)
    {
        Caster = caster;
    }
}
