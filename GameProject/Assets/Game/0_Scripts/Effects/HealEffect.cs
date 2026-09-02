using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HealEffect : Effect
{
    [SerializeField] private float amount;
    [SerializeField] private bool healMySelf;
    public override GameAction GetGameAction(List<Vector2Int> targetpoes, HeroView myView)
    {
        return new HealGA(amount, targetpoes);
    }
}
