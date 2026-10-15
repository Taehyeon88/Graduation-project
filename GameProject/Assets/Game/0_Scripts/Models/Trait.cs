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

    public Trait(TraitData traitData)
    {
        data = traitData;
        condition = traitData.Condition;
        effect = traitData.Effect;

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
    public void Reaction(GameAction gameAction)
    {
        //소유자의 GA에만 반응 (Caster가 없는 GA는 모두 통과)
        if (gameAction is IHaveCaster haveCaster && haveCaster.Caster != owner) return;
        if (!condition.SubConditionIsMat(gameAction)) return;

        effect.PerformReaction(gameAction, owner);
    }
}