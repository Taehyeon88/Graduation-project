using System;
using UnityEngine;

public class ArchorSkeleton : Enemy
{
    public override EnemyAction JudgeActAction(EnemyView enemy)
    {
        Type type = typeof(FireArrowEA);
        var action = GetEnemyAction(enemy, type);
        if (action == null)
            Debug.LogError($"{this}에 {type}라는 행동이 존재하지 않습니다.");

        return action;
    }

    public override bool PerformAction(EnemyView enemy, EnemyAction nextAction)
    {
        if (nextAction is not FireArrowEA fireArrowEA)
            return false;

        return TryMoveIntoRange(enemy, fireArrowEA.Distance);
    }

    public override Enemy Clone()
    {
        return new ArchorSkeleton();
    }
}
