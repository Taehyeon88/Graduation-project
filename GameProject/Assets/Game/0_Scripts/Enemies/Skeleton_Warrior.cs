using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Skeleton_Warrior : Enemy
{
    private const int atkDistance = 1;
    private const int movePoint = 2;
    public override EnemyAction PreJudgeActAction(EnemyView enemy)
    {
        Type type = typeof(NormalAttackEA);
        var action = FindEnemyAction(enemy, type);
        if (action == null)
            Debug.LogError($"{this}에 {type}라는 행동이 존재하지 않습니다.");

        var enemyRM = new E_AllAroundRM();
        var enemyTM = new E_SingleTM();
        var currentPos = TokenSystem.Instance.GetTokenPosition(enemy);

        action.EnemyRM = enemyRM;
        action.EnemyTM = enemyTM;
        action.ActDistance = atkDistance;

        if (!ChangeToWaitEA(enemy, action, currentPos))
        {
            var range = enemyRM.GetGridRanges(currentPos, atkDistance);
            var dirs = enemyTM.GetDirections(range, HeroSystem.Instance.HeroPosition, currentPos, atkDistance);
            action.Directions = dirs;
        }
        else return null;

        return action;
    }

    public override void JudgeAndPlayMove(EnemyView enemy)
    {
        //경로 받기
        //행동 실행
        var path = new List<Vector2Int>();
        int distance = TokenSystem.Instance.GetDistance(enemy, HeroSystem.Instance.HeroView);
        if (distance == 1)
        {
            return;
        }
        else
        {
            var targetPoses = GetAllPlaceByOne(enemy);
            if (targetPoses.Count > 0)
            {
                int r_value = UnityEngine.Random.Range(0, targetPoses.Count);
                Vector2Int targetPos = targetPoses[r_value];
                path = TokenSystem.Instance.GetShortestPath(enemy, targetPos);
            }
            else
            {
                path = GetMinValue(enemy);
            }
        }

        if (path != null)
        {
            //Debug.Log(string.Join(", ", path));
            //개수 2 이상일 겨우, 나머지는 컷
            if (path.Count > movePoint)
                path.RemoveRange(movePoint, path.Count - movePoint);

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
                var pos = TokenSystem.Instance.GetPositionByDirection(enemy, dir);
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
        return new Skeleton_Warrior();
    }

    //Privates
    private List<Vector2Int> GetAllPlaceByOne(EnemyView enemy)
    {
        const int D_detect = 1;

        Vector2Int heroPos = HeroSystem.Instance.HeroPosition;
        Vector2Int myPos = TokenSystem.Instance.GetTokenPosition(enemy);

        var allPlaces = TokenSystem.Instance.GetAllAroundPlaces(heroPos, D_detect);
        foreach (var place in allPlaces.ToList())
        {
            int dis = TokenSystem.Instance.GetDistance(heroPos, place);
            int cost = TokenSystem.Instance.GetDistance(myPos, place);

            if (cost > movePoint)
                allPlaces.Remove(place);
        }

        return allPlaces;
    }

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
