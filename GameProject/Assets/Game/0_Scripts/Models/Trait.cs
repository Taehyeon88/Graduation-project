using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trait
{
    public Sprite Image => data.Image;
    public string Description => data.Description;
    public string Title { get; private set; }

    private readonly TraitData data;
    private readonly TraitCondition condition;
    private readonly TraitEffect effect;
    protected CombatantView owner;   //전투용 데이터
    private int triggerCount;        //발동 횟수 (ILimitedTrigger 효과의 상한 검사용, 유닛별 상태라 Trait이 보유)

    public Trait(TraitData traitData)
    {
        data = traitData;
        condition = traitData.Condition.Clone();   //SO 공유를 피하려고 유닛별 사본을 보유
        effect = traitData.Effect.Clone();

        string name = data.name;
        int index = name.IndexOf("_");
        Title = index >= 0 ? name.Substring(index + 1) : name;
    }
    public void SetOwner(CombatantView owner)
    {
        this.owner = owner; 
    }
    public void OnAdd()
    {
        condition.SubscribeCondition(Reaction);
    }
    public void OnRemove()
    {
        condition.UnsubscribeCondition(Reaction);
    }
    //효과가 자동 전투 행동을 대체하면 true (대체했을 때만 기본 행동을 건너뜀)
    public bool TryOverrideBattle()
    {
        return effect is IBattleOverride overrider && overrider.TryBattle(owner);
    }
    public void Reaction(GameAction gameAction)
    {
        //소유자의 GA에만 반응 (Caster가 없는 GA는 모두 통과)
        if (condition.FiltersByCaster && gameAction is IHaveCaster haveCaster && haveCaster.Caster != owner) return;
        if (!condition.SubConditionIsMat(gameAction, owner)) return;
        if (effect is ILimitedTrigger limited && limited.MaxTriggers > 0 && triggerCount >= limited.MaxTriggers) return;

        triggerCount++;
        effect.PerformReaction(gameAction, owner);
    }
}