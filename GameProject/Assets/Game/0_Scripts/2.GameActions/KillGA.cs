using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillGA : GameAction
{
    public Token Token { get; private set; }
    public KillGA(Token token)
    {
        Token = token;
    }
}
