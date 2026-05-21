using UnityEngine;

public class PowerTotemEmissionEffect : Effect
{
    [SerializeField] private int damage;
    public override GameAction GetGameAction(EffectInfo effectInfo)
    {
        return new PowerTotemEmissionGA(damage);
    }
}
