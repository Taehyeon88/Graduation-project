using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Card
{
    public string Description => data.Description;
    public Sprite Image => data.Image;
    public CardType CardType => data.CardType;
    public CardSubType CardSubType => data.CardSubType;
    public List<Effect> SelfEffects => data.SelfEffects;
    public GridTargetMode GridTargetMode => data.GridTargetMode;
    public string Title { get; private set; }
    public int Mana { get; private set; }
    public bool LockDiscarding { get; set; }

    private readonly CardData data;

    public Card(CardData data)
    {
        this.data = data;
        Mana = data.Mana;
        LockDiscarding = data.LockDiscarding;

        string name = data.name;
        int index = name.IndexOf("_");
        Title = index >= 0? name.Substring(index + 1) : name;
    }
}
