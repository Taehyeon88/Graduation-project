using System.Collections.Generic;

public class ModifyStatGA : GameAction, IHaveCaster
{
    public StatType StatType { get; private set; }
    public int Amount { get; private set; }   //증감량 (음수 가능)
    public List<CombatantView> Targets { get; private set; }
    public CombatantView Caster { get; private set; }
    Token IHaveCaster.Caster => Caster;

    public ModifyStatGA(StatType statType, int amount, List<CombatantView> targets, CombatantView caster)
    {
        StatType = statType;
        Amount = amount;
        Targets = targets;
        Caster = caster;
    }
}
