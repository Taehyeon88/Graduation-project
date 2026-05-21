using UnityEngine;

public class PowerTotemEmissionGA : GameAction
{
    public int Damage { get; private set; }
    public PowerTotemEmissionGA(int damage)
    {
        Damage = damage;
    }
}
