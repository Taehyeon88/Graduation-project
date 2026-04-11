using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class EnemyActionInfo
{
    //행동
    public Type actType;
    public EnemyRangeMode enemyRM;
    public int actDistance;
    public bool isPenetration;

    //이동
    public List<Vector2Int> movePath;

    public EnemyActionInfo(EnemyActionInfo actionInfo, List<Vector2Int> movePath)
    {
        this.actType = actionInfo.actType;
        this.enemyRM = actionInfo.enemyRM;
        this.actDistance = actionInfo.actDistance;
        this.isPenetration = actionInfo.isPenetration;
        this.movePath = movePath;
    }

    public EnemyActionInfo() { }

    //Constructor
    public void SetActionInfo(Type type, EnemyRangeMode enemyRangeMode, int actDistance, bool isPeneration)
    {
        this.actType = type;
        this.enemyRM = enemyRangeMode;
        this.actDistance = actDistance;
        this.isPenetration = isPeneration;
    }

    public void SetMoveInfo(List<Vector2Int> movePath)
    {
        this.movePath = movePath;
    }
}
