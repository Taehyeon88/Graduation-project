using System;
using UnityEngine;

public class Larva : Enemy
{
    public override EnemyAction JudgeActAction(EnemyView enemy)
    {
        Type type = typeof(AttackEA);
        var action = GetEnemyAction(enemy, type);
        if (action == null)
            Debug.LogError($"{this}에 {type}라는 행동이 존재하지 않습니다.");

        return action;
    }
    public override bool PerformAction(EnemyView enemy, EnemyAction nextAction)
    {
        //공격 가능 범위 = 이동 해서 공격 사거리 안에 들어오는가?

        //공격 가능 범위 내의 공격 가능한 모든 적 찾기

        //인접 1칸 내, 영웅 찾아서 위치 정보 전달
        // if (nextAction is AttackEA attackEA)
        // {
        //     var myPos = TokenSystem.Instance.API.GetTokenPosition(enemy);
        //     var poses = TokenSystem.Instance.API.GetAllAroundPlaces(myPos, attackEA.Distance, false, true);
        //     if (poses != null && poses.Count > 0)
        //     {
        //         foreach (Vector2Int p in poses)
        //         {
        //             Token token = TokenSystem.Instance.API.GetTokenByPosition(p);
        //             if (token != null && token is HeroView hero)
        //             {
        //                 Debug.Log($"토큰 이름{token.TokenData.Name}, 위치{p}, 사거리{attackEA.Distance}");
        //                 attackEA.targetPosition = p;
        //                 return true;
        //             }
        //         }
        //     }
        // }
        // return false;
        return true;
    }

    public override Enemy Clone()
    {
        return new Scarecrow();
    }
}
