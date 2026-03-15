using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayCardGA : GameAction
{
    public Card Card { get; set; }
    public bool IsPart1 { get; set; }   //Part1, Part2가 있음. 각각 SelfEfects 와 GridTargetMode를 담당.
    public PlayCardGA(Card card, bool isPart1 = true)
    {
        Card = card;
        IsPart1 = isPart1;
    }
}
