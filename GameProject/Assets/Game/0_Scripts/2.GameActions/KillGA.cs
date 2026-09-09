using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillGA : GameAction
{
    public Token Token { get; private set; }
    public Tween Hit_Tween { get; private set; }
    public KillGA(Token token, Tween hit_Tween)
    {
        Token = token;
        Hit_Tween = hit_Tween;
    }
}
