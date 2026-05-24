using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SeverRat : Enemy
{
    private const int attackDistance = 1;  //공격 사거리

    public override EnemyAction PreJudgeActAction(EnemyView myEnemyView)
    {
        Type type = typeof(NormalAttackEA);
        var action = FindEnemyAction(myEnemyView, type);
        if (action == null)
            Debug.LogError($"{this}에 {type}라는 행동이 존재하지 않습니다.");

        var enemyRM = new E_AllAroundRM();
        var currentPos = TokenSystem.Instance.GetTokenPosition(myEnemyView);

        int minDis = int.MaxValue;
        var dir = Vector2Int.zero;
        foreach (var pos in enemyRM.GetGridRanges(currentPos, attackDistance, false))
        {
            //플레이어와 가장 가까운 위치를 공격범위로 선정
            int distance = TokenSystem.Instance.GetDistance(TokenSystem.Instance.HeroView, pos);
            if (distance < minDis)
            {
                minDis = distance;
                dir = pos - currentPos;
            }
        }

        action.Directions.Add(dir);
        action.ActDistance = attackDistance;
        action.IsPenetration = false;

        return action;
    }

    public override void SetDrawActActionVG(bool active, EnemyView enemy, EnemyAction enemyAction)
    {
        if (active)
        {
            VisualGridCreator.Instance.RemoveVisualGrid(enemy.gameObject.GetInstanceID(), "Enemy_Attack");

            foreach (var dir in enemyAction.Directions)
            {
                var pos = TokenSystem.Instance.GetDirectionPos(enemy, dir);
                VisualGridCreator.Instance.CreateVisualGrid(enemy.gameObject.GetInstanceID(), pos, "Enemy_Attack");
            }
        }
        else
        {
            VisualGridCreator.Instance.RemoveVisualGridById(enemy.gameObject.GetInstanceID());
        }
    }

    public override void JudgeAndPlayMove(EnemyView enemy)
    {
        //경로 받기
        //행동 실행

        var path = GetMinValue(enemy);
        if (path != null)
        {
            PerformMoveGA performMoveGA = new(enemy, path);
            ActionSystem.Instance.AddReaction(performMoveGA);
        }
        else
        {
            Debug.LogWarning($"몬스터_{enemy.name}이 다음으로 이동할 경로를 찾을 수 없음");
        }
    }

    public override EnemyAction ReCalculate(EnemyView enemy) { return null; }

    public override Enemy Clone()
    {
        return new SeverRat();
    }


    //Privates
    private List<Vector2Int> GetMinValue(EnemyView enemy)
    {
        //플레이어 주위 타일 받기
        //영웅과의 최소 거리의 타일 선정
        //해당 타일로 최소 경로 찾기

        int minDistance = int.MaxValue;
        Vector2Int targetPos = default;
        const int D_detect = 3;

        Vector2Int heroPos = HeroSystem.Instance.HeroPosition;
        Vector2Int myPos = TokenSystem.Instance.GetTokenPosition(enemy);

        var allPlaces = TokenSystem.Instance.GetAllAroundPlaces(heroPos, D_detect);
        foreach (var place in allPlaces)
        {
            int dis = TokenSystem.Instance.GetDistance(heroPos, place);

            if (dis < minDistance)
            {
                minDistance = dis;
                targetPos = place;
            }
            else if (dis == minDistance)
            {
                //30퍼센트 확률로 바뀌게 설정
                float r_value = UnityEngine.Random.value;
                if (r_value <= 0.3f)
                {
                    targetPos = place;
                }
            }
        }

        if (minDistance == int.MaxValue) return null;

        return TokenSystem.Instance.GetShortestPath(enemy, targetPos);
    }
}
