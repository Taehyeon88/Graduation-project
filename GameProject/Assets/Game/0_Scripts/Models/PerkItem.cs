using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerkItem
{
    public Sprite Image => data.Image;
    public string Description => data.Description;
    public string Title { get; private set; }

    private readonly PerkData data;
    private readonly Perk perk;
    protected HeroView owner;   //전투용 데이터

    public PerkItem(PerkData perkData)
    {
        data = perkData;
        perk = perkData.Perk;

        string name = data.name;
        int index = name.IndexOf("_");
        Title = index >= 0 ? name.Substring(index + 1) : name;
    }
    public void SetOwner(HeroView owner)
    {
        this.owner = owner; 
    }
    public void OnAdd()
    {
        perk.SubscribeCondition(Reaction);
    }
    public void OnRemove()
    {
        perk.UnsubscribeCondition(Reaction);
    }
    public void Reaction(GameAction gameAction)
    {
        if (!perk.SubConditionIsMat(gameAction, owner)) return;

        perk.PerformReaction(gameAction, owner);
    }
}