using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillGA : GameAction
{
    public Token Token { get; private set; }
    public Tween Hit_Tween { get; private set; }
    public Token Killer { get; private set; }   //처치한 토큰 (스킬 등 시전자가 없으면 null)
    public KillGA(Token token, Tween hit_Tween, Token killer = null)
    {
        Token = token;
        Hit_Tween = hit_Tween;
        Killer = killer;
    }
}
