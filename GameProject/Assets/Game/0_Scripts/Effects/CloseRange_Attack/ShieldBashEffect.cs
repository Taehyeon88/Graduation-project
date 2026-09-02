using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShieldBashEffect : Effect
{
    [SerializeField] private float amount;
    public override GameAction GetGameAction(List<Vector2Int> targetpoes, HeroView myView)
    {
        var shieldBashGA = new ShieldBashGA(amount, targetpoes, myView);
        return shieldBashGA;
    }
}
