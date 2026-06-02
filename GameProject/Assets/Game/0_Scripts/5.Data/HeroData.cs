using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Token/Hero")]
public class HeroData : TokenData
{
    [field : SerializeField] public List<CardData> Deck { get; private set; }
    [field : SerializeField] public int Gold { get; private set; }
}
