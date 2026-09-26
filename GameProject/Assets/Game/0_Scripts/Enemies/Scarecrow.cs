using System;
using UnityEngine;

public class Scarecrow : Enemy
{
    public override EnemyAction JudgeActAction(EnemyView enemy)
    {
        Type type = typeof(AttackEA);
        var action = GetEnemyAction(enemy, type);
        if (action == null)
            Debug.LogError($"{this}에 {type}라는 행동이 존재하지 않습니다.");

        //공격력, 거리 동적 설정 가능

        return action;
    }
    public override bool PerformAction(EnemyView enemy, EnemyAction nextAction)
    {
        //대상 탐색은 PlayEnemyAction이 스스로 처리 (이동 없는 몬스터라 재검증할 것도 없음)
        return true;
    }

    public override Enemy Clone()
    {
        return new Scarecrow();
    }
}
