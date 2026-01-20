using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    public string Title => data.name;
    public string Description => data.Description;
    public Sprite Image => data.Image;
    public int Mana { get; private set; }

    private readonly CardData data;

    public Card(CardData data)
    {
        this.data = data;
        Mana = data.Mana;
    }
}
