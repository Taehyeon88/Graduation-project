using System;
using UnityEngine;

[System.Serializable]
public abstract class TraitCondition
{
    [SerializeField] protected ReactionTiming reactionTiming;
    public abstract void SubscribeCondition(Action<GameAction> reaction);
    public abstract void UnsubscribeCondition(Action<GameAction> reaction);
    public abstract bool SubConditionIsMat(GameAction action, CombatantView owner);   //owner는 판정용 인자

    //true면 Caster가 소유자인 GA에만 반응 (피격 반응처럼 상대가 Caster인 GA에 반응하는 조건은 false로 오버라이드)
    public virtual bool FiltersByCaster => true;

    //Trait이 유닛별 사본을 갖기 위한 얕은 복사 (참조형 상태를 가진 조건은 오버라이드해서 새로 만든다)
    public virtual TraitCondition Clone() => (TraitCondition)MemberwiseClone();
}
