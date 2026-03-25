using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnockBackGA : GameAction
{
    public CombatantView Caster { get; private set; } //시전자
    public int Distance { get; private set; }         //넉백 효과(밀려나는) 거리
    public bool IsSingle { get; private set; }        //단일 대상 지정 여부 

    //Single - Target
    public Vector2Int TargetPos { get; private set; }  //피격자
    public Vector2Int Direction { get; private set; }  //넉백 시킬 방향

    //Multi - Target
    public List<Vector2Int> TargetPoses { get; private set; }  //피격자들
    public List<Vector2Int> Directions { get; private set; }   //넉백 시킬 방향들

    public KnockBackGA(CombatantView caster, int distance, Vector2Int targetPos, Vector2Int direction)
    {
        Caster = caster;
        Distance = distance;
        TargetPos = targetPos;
        Direction = direction;
        IsSingle = true;
    }

    public KnockBackGA(CombatantView caster, int distance, List<Vector2Int> targetPoses, List<Vector2Int> directions)
    {
        Caster = caster;
        Distance = distance;
        TargetPoses = targetPoses;
        Directions = directions;
        IsSingle = false;
    }
}
