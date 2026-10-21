using System;
using UnityEngine;

[System.Serializable]
public abstract class PerkCondition
{
    [SerializeField] protected ReactionTiming reactionTiming;
    public abstract void SubscribeCondition(Action<GameAction> reaction);
    public abstract void UnsubscribeCondition(Action<GameAction> reaction);
    public abstract bool SubConditionIsMat(GameAction action);

    //Perk이 사본을 갖기 위한 얕은 복사 (참조형 상태를 가진 조건은 오버라이드해서 새로 만든다)
    public virtual PerkCondition Clone() => (PerkCondition)MemberwiseClone();
}
