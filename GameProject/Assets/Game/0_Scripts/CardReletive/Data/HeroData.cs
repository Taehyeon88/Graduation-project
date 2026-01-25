using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Hero")]
public class HeroData : TokenData
{
    [field : SerializeField] public int Health { get; private set; }
    [field : SerializeField] public List<CardData> Deck { get; private set; }
}
