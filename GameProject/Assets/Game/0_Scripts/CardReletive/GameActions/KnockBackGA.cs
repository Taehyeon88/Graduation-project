using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnockBackGA : GameAction
{
    public Vector2Int TargetPos { get; private set; }  //피격자
    public CombatantView Caster { get; private set; } //시전자
    public int Distance { get; private set; }          //넉백 효과(밀려나는) 거리
    public Vector2Int Direction { get; private set; }  //넉백 시킬 방향

    public KnockBackGA(CombatantView caster, int distance, Vector2Int targetPos, Vector2Int direction)
    {
        Caster = caster;
        Distance = distance;
        TargetPos = targetPos;
        Direction = direction;
    }
}
