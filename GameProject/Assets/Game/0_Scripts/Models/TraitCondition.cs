using System;
using UnityEngine;

[System.Serializable]
public abstract class TraitCondition
{
    [SerializeField] protected ReactionTiming reactionTiming;
    public abstract void SubscribeCondition(Action<GameAction> reaction);
    public abstract void UnsubscribeCondition(Action<GameAction> reaction);
    public abstract bool SubConditionIsMat(GameAction action, CombatantView owner);   //owner는 판정용 인자 (조건은 SO 공유라 저장 금지)
}
