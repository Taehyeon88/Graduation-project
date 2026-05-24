using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroFirstMoveGA : GameAction
{
    public int SPD { get; private set; }
    public HeroFirstMoveGA(int SPD)
    {
        this.SPD = SPD;
    }
}
