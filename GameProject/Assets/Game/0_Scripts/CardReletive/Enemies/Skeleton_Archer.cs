using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Skeleton_Archer : Enemy
{
    private const int atkDistance = 2;
    private const int movePoint = 2;
    public override EnemyAction PreJudgeActAction(EnemyView enemy)
    {
        Type type = typeof(NormalAttackEA);
        var action = FindEnemyAction(enemy, type);
        if (action == null)
            Debug.LogError($"{this}에 {type}라는 행동이 존재하지 않습니다.");

        var enemyRM = new E_AllAroundRM();
        var currentPos = TokenSystem.Instance.GetTokenPosition(enemy);

        action.EnemyRM = enemyRM;
        action.ActDistance = atkDistance;
        action.IsPenetration = false;

        if (!ChangeToWaitEA(enemy, action, currentPos))
        {
            Vector2Int dir = HeroSystem.Instance.HeroPosition - currentPos;
            action.Directions.Add(dir);
        }
        else return null;

        return action;
    }

    public override void JudgeAndPlayMove(EnemyView enemy)
    {
        //은엄폐 구역 찾기(인접2)
        //해당 구역 내, 이동 가능한 구역 존재? -> y : 해당 타일로 이동(다수일 경우, n등분 확률적용)
        //-> n : 현재 인접 2칸내인가? -> y : 이동X
        //-> n : 은엄폐 구역이 존재하는가? -> y: 목표로 이동
        //-> n : 플레이어를 목표로 이동

        Vector2Int heroPos = HeroSystem.Instance.HeroPosition;
        Vector2Int myPos = TokenSystem.Instance.GetTokenPosition(enemy);
        Vector2Int targetPos = default;
        var path = new List<Vector2Int>();

        //은엄폐 구역 찾기(인접2)
        var coverPlaces = new List<Vector2Int>();
        var places = TokenSystem.Instance.GetAllAroundPlaces(heroPos, atkDistance);
        foreach (var place in places)
        {
            if (TokenSystem.Instance.GetMinDistance(HeroSystem.Instance.HeroView, place) >= atkDistance + 1)
                coverPlaces.Add(place);
        }

        //Debug.Log($" 엄폐구역 : {string.Join(",", coverPlaces)}");

        //해당 구역 내, 이동 가능한 구역 존재? -> y : 해당 타일로 이동(다수일 경우, n등분 확률적용)
        var targetPoses = coverPlaces.FindAll(
            pos => TokenSystem.Instance.GetDistance(pos, myPos) <= movePoint
            );

        //Debug.Log($" 엄폐 + 이동 가능 구역 : {string.Join(",", targetPoses)}");
        if (coverPlaces.Count > 0 && targetPoses.Count > 0)
        {
            int r_value = UnityEngine.Random.Range(0, targetPoses.Count);
            targetPos = targetPoses[r_value];
            path = TokenSystem.Instance.GetShortestPath(enemy, targetPos);

            //Debug.Log("시퀀스 1 실행");
        }
        else //-> n : 현재 인접 2칸내인가? -> y : 이동X
        {
            int dis = TokenSystem.Instance.GetDistance(heroPos, myPos);
            if (dis <= atkDistance)
            {
                //Debug.Log("시퀀스 2 실행");
                return;
            }
            else //-> n : 플레이어와 인접2칸 거리로 이동할 수 있는가? => y : 해당 타일로 이동
            {
                var allPlacesInTwo = GetAllPlaceByDistance(enemy, atkDistance);
                if (allPlacesInTwo.Count > 0)
                {
                    int r_value = UnityEngine.Random.Range(0, targetPoses.Count);
                    targetPos = allPlacesInTwo[r_value];
                    path = TokenSystem.Instance.GetShortestPath(enemy, targetPos);
                    //Debug.Log("시퀀스 3 실행");
                }
                else //-> n : 은엄폐 구역이 존재하는가? -> y: 목표로 이동
                {
                    if (coverPlaces.Count > 0)
                    {
                        int r_value = UnityEngine.Random.Range(0, coverPlaces.Count);
                        targetPos = coverPlaces[r_value];
                        path = TokenSystem.Instance.GetShortestPath(enemy, targetPos);
                        //Debug.Log("시퀀스 4 실행");
                    }
                    else //-> n : 플레이어를 목표로 이동
                    {
                        path = GetMinValue(enemy);
                        //Debug.Log("시퀀스 5 실행");
                    }
                }
            }
        }

        if (path != null)
        {
            //개수 2 이상일 겨우, 나머지는 컷
            if (path.Count > movePoint)
                path.RemoveRange(2, path.Count - movePoint);

            PerformMoveGA performMoveGA = new(enemy, path);
            ActionSystem.Instance.AddReaction(performMoveGA);
        }
        else
        {
            Debug.LogWarning($"몬스터_{enemy.name}이 다음으로 이동할 경로를 찾을 수 없음");
        }
    }

    public override void SetDrawActActionVG(bool active, EnemyView enemy, EnemyAction action)
    {
        if (active)
        {
            VisualGridCreator.Instance.RemoveVisualGrid(enemy.gameObject.GetInstanceID(), "Enemy_Attack");

            foreach (var dir in action.Directions)
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

    public override EnemyAction ReCalculate(EnemyView enemy) { return null; }

    public override Enemy Clone()
    {
        return new Skeleton_Archer();
    }

    //Privates

    private List<Vector2Int> GetAllPlaceByDistance(EnemyView enemy, int distance)
    {
        const int D_detect = 3;

        Vector2Int heroPos = HeroSystem.Instance.HeroPosition;
        Vector2Int myPos = TokenSystem.Instance.GetTokenPosition(enemy);

        var allPlaces = TokenSystem.Instance.GetAllAroundPlaces(heroPos, D_detect);
        foreach (var place in allPlaces.ToList())
        {
            int dis = TokenSystem.Instance.GetDistance(heroPos, place);
            int cost = TokenSystem.Instance.GetDistance(myPos, place);

            if (dis != distance || cost > movePoint)
                allPlaces.Remove(place);
        }

        return allPlaces;
    }

    private List<Vector2Int> GetMinValue(EnemyView enemy)
    {
        //플레이어 주위 타일 받기
        //영웅과의 최소 거리의 타일 선정
        //해당 타일로 최소 경로 찾기

        int minCost = int.MaxValue;
        Vector2Int targetPos = default;
        const int D_detect = 3;

        Vector2Int heroPos = HeroSystem.Instance.HeroPosition;
        Vector2Int myPos = TokenSystem.Instance.GetTokenPosition(enemy);

        var allPlaces = TokenSystem.Instance.GetAllAroundPlaces(heroPos, D_detect);

        foreach (var place in allPlaces)
        {
            int dis = TokenSystem.Instance.GetDistance(heroPos, place);
            int cost = TokenSystem.Instance.GetDistance(myPos, place);

            if (dis < atkDistance) continue;

            if (cost < minCost)
            {
                minCost = cost;
                targetPos = place;
            }
            else if (cost == minCost)
            {
                //30퍼센트 확률로 바뀌게 설정
                float r_value = UnityEngine.Random.value;
                if (r_value <= 0.3f)
                {
                    targetPos = place;
                }
            }
        }

        if (minCost == int.MaxValue) return null;

        return TokenSystem.Instance.GetShortestPath(enemy, targetPos);
    }
}
