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
    public int RepeatCnt { get; private set; }
    public int Available_Cnt { get; private set; }
    public int Max_Available_Cnt { get; private set; }

    public readonly CardData data;

    public Card(CardData data)
    {
        this.data = data;
        Mana = data.Mana;
        RepeatCnt = data.RepeatCnt;
        Available_Cnt = Max_Available_Cnt = 1;

        string name = data.name;
        int index = name.IndexOf("_");
        Title = index >= 0? name.Substring(index + 1) : name;
    }

    public void ReduceAvailable_Cnt()
    {
        Available_Cnt--;
    }
    public void RefillAvailable_Cnt()
    {
        Available_Cnt = Max_Available_Cnt;
    }

    public void ReduceRepeatCnt()
    {
        RepeatCnt = Mathf.Max(RepeatCnt--, 1);
    }
}
