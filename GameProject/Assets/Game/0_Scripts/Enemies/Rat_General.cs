using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Rat_General : Enemy
{
    private const int movePoint = 2;
    public override EnemyAction PreJudgeActAction(EnemyView enemy)
    {
        //타겟 범위 == 1? (인접 1칸) -> 확률 = 50%? -> 대검 휘두리기
        Vector2Int currentPos = TokenSystem.Instance.GetTokenPosition(enemy);
        Vector2Int heroPos = HeroSystem.Instance.HeroPosition;

        EnemyRangeMode enemyRM;  EnemyTargetMode enemyTM;
        List<Vector2Int> range;  Type type;
        EnemyAction action;

        int dis = Mathf.Max(Mathf.Abs(currentPos.x - heroPos.x), Mathf.Abs(currentPos.y - heroPos.y));
        if (dis <= 1)
        {
            float r_value = UnityEngine.Random.value;
            if (r_value <= 0.5f)
            {
                //대검 휘두리기 실행
                type = typeof(RatSwingGreatsSwordEA);
                action = FindEnemyAction(enemy, type);
                if (action == null)
                    Debug.LogError($"{this}에 {type}라는 행동이 존재하지 않습니다.");

                enemyRM = new E_AllEightAroundRM();
                enemyTM = new E_ConeTM();

                range = enemyRM.GetGridRanges(currentPos, 1);
                if (range.Contains(heroPos))
                {
                    var dirs = enemyTM.GetDirections(range, heroPos, currentPos, 1);
                    action.EnemyRM = enemyRM;
                    action.EnemyTM = enemyTM;
                    action.ActDistance = 1;
                    action.Directions = dirs;
                    return action;
                }
            }
        }

        //타겟 범위 <= 2? (X 2칸 이내) -> 공격 범위 내 첫 1칸에 몬스터가 없을 시 -> 돌진
        type = typeof(RatDashEA);
        action = FindEnemyAction(enemy, type);
        if (action == null)
            Debug.LogError($"{this}에 {type}라는 행동이 존재하지 않습니다.");

        enemyRM = new E_CrossRM();
        enemyTM = new E_LineOneTM();

        range = enemyRM.GetGridRanges(currentPos, 2);
        if (range.Contains(heroPos))
        {
            var dir2s = enemyTM.GetDirections(range, heroPos, currentPos, 2);
            Vector2Int targetPos = currentPos + dir2s[0];
            if (TokenSystem.Instance.IsGridEmpty(targetPos, false, true, true))
            {
                action.EnemyRM = enemyRM;
                action.EnemyTM = enemyTM;
                action.ActDistance = 2;
                action.Directions = dir2s;
                return action;
            }
        }

        //타겟 범위 2칸 이내 -> 돌던지기
        type = typeof(RatThrowingStoneEA);
        action = FindEnemyAction(enemy, type);
        if (action == null)
            Debug.LogError($"{this}에 {type}라는 행동이 존재하지 않습니다.");

        enemyRM = new E_AllAroundRM();
        enemyTM = new E_SingleTM();

        action.EnemyRM = enemyRM;
        action.EnemyTM = enemyTM;
        action.ActDistance = 2;

        range = enemyRM.GetGridRanges(currentPos, 2);
        if (range.Contains(heroPos))
        {
            var dir3s = enemyTM.GetDirections(range, heroPos, currentPos, 2);
            action.Directions = dir3s;
            return action;
        }

        WaitEA waitEA = FindEnemyAction(enemy, typeof(WaitEA)) as WaitEA;
        waitEA.ReservedEA = action;
        return waitEA;
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
            Debug.Log(string.Join(", ", path));
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

    public override EnemyAction ReCalculate(EnemyView enemy) { return null; }

    public override Enemy Clone()
    {
        return new Rat_General();
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
