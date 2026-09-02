using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class Effect
{
    public abstract GameAction GetGameAction(List<Vector2Int> targetpoes, HeroView myView);
}
