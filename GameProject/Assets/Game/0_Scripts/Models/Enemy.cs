using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public abstract class Enemy
{
    public abstract EnemyAction JudgeActAction(EnemyView enemy);        //다음 할 행동 미리 판단
    public abstract bool PerformAction(EnemyView enemy, EnemyAction nextAction);  //몬스터 행동 실행 함수
    public abstract Enemy Clone();                                      //복사 함수


    //특정 Action을 찾아서 받는 함수
    protected EnemyAction GetEnemyAction(EnemyView enemy, Type type)
    {
        foreach (var action in enemy.Actions)
        {
            if(action.GetType() == type) return action;
        }
        return null;
    }

    //가장 가까운 대상 방향으로 사거리 안까지 이동 판단(동률 랜덤). 이미 사거리 안이면 이동 없이 true,
    //이동해서 들어갈 수 있으면 PerformMoveGA 큐잉 후 true, 이동력이 부족하면 갈 수 있는 만큼만 전진 후 false.
    protected bool TryMoveIntoRange(EnemyView enemy, int distance)
    {
        var heroes = TokenSystem.Instance.HeroViews
            .Where(h => h != null && h.CurrentHealth > 0)
            .ToList();
        if (heroes.Count == 0)
            return false;

        Vector2Int myPos = TokenSystem.Instance.API.GetTokenPosition(enemy);

        int minDist = heroes.Min(h => TokenSystem.Instance.API.GetDistance(myPos, TokenSystem.Instance.API.GetTokenPosition(h)));
        var nearestHeroes = heroes
            .Where(h => TokenSystem.Instance.API.GetDistance(myPos, TokenSystem.Instance.API.GetTokenPosition(h)) == minDist)
            .ToList();

        HeroView target = nearestHeroes.Count == 1
            ? nearestHeroes[0]
            : nearestHeroes[UnityEngine.Random.Range(0, nearestHeroes.Count)];

        Vector2Int targetPos = TokenSystem.Instance.API.GetTokenPosition(target);

        if (TokenSystem.Instance.API.GetDistance(myPos, targetPos) <= distance)
        {
            return true;
        }

        var movable = TokenSystem.Instance.API.GetCanMovePlace(enemy, enemy.CurrentMovePoint);
        var candidates = movable
            .Where(p => TokenSystem.Instance.API.GetDistance(p, targetPos) <= distance)
            .ToList();

        if (candidates.Count > 0)
        {
            List<Vector2Int> bestPath = null;
            foreach (Vector2Int c in candidates)
            {
                var path = TokenSystem.Instance.API.GetShortestPath(enemy, c);
                if (path == null) continue;
                if (bestPath == null || path.Count < bestPath.Count)
                    bestPath = path;
            }

            if (bestPath != null)
            {
                enemy.SpendMovePoint(bestPath.Count);
                ActionSystem.Instance.AddReaction(new PerformMoveGA(enemy, bestPath));
                return true;
            }
        }

        Vector2Int? closest = null;
        List<Vector2Int> closestPath = null;
        foreach (Vector2Int p in movable)
        {
            var path = TokenSystem.Instance.API.GetShortestPath(enemy, p);
            if (path == null) continue;
            if (closest == null ||
                TokenSystem.Instance.API.GetDistance(p, targetPos) < TokenSystem.Instance.API.GetDistance(closest.Value, targetPos))
            {
                closest = p;
                closestPath = path;
            }
        }

        if (closestPath != null)
        {
            enemy.SpendMovePoint(closestPath.Count);
            ActionSystem.Instance.AddReaction(new PerformMoveGA(enemy, closestPath));
        }

        return false;
    }
}
