using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Card
{
    public string Title => data.name.Contains("_")? data.name.Substring(data.name.IndexOf("_")+1) : data.name;
    public string Description => data.Description;
    public Sprite Image => data.Image;
    public CardType CardType => data.CardType;
    public CardTypeType CardTypeType => data.CardTypeType;
    public List<Effect> SelfEffects => data.SelfEffects;
    public GridTargetMode GridTargetMode => data.GridTargetMode;
    public int Mana { get; private set; }

    private readonly CardData data;

    public Card(CardData data)
    {
        this.data = data;
        Mana = data.Mana;
    }
}
