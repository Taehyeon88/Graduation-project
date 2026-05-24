using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Hero")]
public class HeroData : TokenData
{
    [field : SerializeField] public List<CardData> Deck { get; private set; }
}
