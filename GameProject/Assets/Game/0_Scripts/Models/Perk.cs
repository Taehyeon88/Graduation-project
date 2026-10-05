using System;
using UnityEngine;

[System.Serializable]
public abstract class Perk
{
    public abstract void SubscribeCondition(Action<GameAction> reaction);
    public abstract void UnsubscribeCondition(Action<GameAction> reaction);
    public abstract bool SubConditionIsMat(GameAction action, CombatantView owner);
    public abstract void PerformReaction(GameAction action, CombatantView owner);
}
