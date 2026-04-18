using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class ParasolMushroom : Enemy
{

    public override EnemyAction PreJudgeActAction(EnemyView myEnemyView)
    {
        Type type = typeof(NormalAttackEA);
        var action = FindEnemyAction(myEnemyView, type);
        if (action == null)
            Debug.LogError($"{this}에 {type}라는 행동이 존재하지 않습니다.");

        var enemyRM = new E_SnowRM();
        var currentPos = TokenSystem.Instance.GetTokenPosition(myEnemyView);
        var targetPos = TokenSystem.Instance.GetTokenPosition(HeroSystem.Instance.HeroView);

        var range = enemyRM.GetGridRanges(currentPos, 4, true);

        Vector2Int direction = Vector2Int.zero;
        Vector2Int dir = targetPos - currentPos;
        int absX = Mathf.Abs(dir.x);    int absY = Mathf.Abs(dir.y);
        if (absX == absY || absX == 0 || absY == 0)
        {
            if (absY == 0)
                direction = new(dir.x / absX, 0);
            else if (absX == 0)
                direction = new(0, dir.y / absY);
            else
                direction = new(dir.x / absX, dir.y / absY);
        }
        else
        {
            int min = Mathf.Min(absX, absY); int max = Mathf.Max(absX, absY);
            if (max - min >= min)
            {
                direction = new(dir.x / absX, dir.y / absY);
            }
            else
            {
                if (min == absX) direction = new(0, dir.y / absY);
                else if (min == absY) direction = new(dir.x / absX, 0);
            }
        }

        for (int i = 1; i <= 4; i++)
        {
            action.Directions.Add(direction * i);
        }

        action.ActDistance = 4;
        action.IsPenetration = false;

        return action;
    }

    public override List<Vector2Int> PreJudgeMoveAction(EnemyView enemy)
    {
        var heroPos = TokenSystem.Instance.GetTokenPosition(HeroSystem.Instance.HeroView);
        var curPos = TokenSystem.Instance.GetTokenPosition(enemy);

        //현재 위치가 공격 위치일 경우, 이동 안함 처리
        if (Mathf.Abs((heroPos - curPos).x) == Mathf.Abs((heroPos - curPos).y) 
            || (heroPos - curPos).x == 0 
            || (heroPos - curPos).y == 0)
        {
            return null;
        }

        //공격 하기 좋은 위치 찾기 처리
        var range = TokenSystem.Instance.GetCanMovePlace(enemy, 3);
        Vector2Int targetPos = Vector2Int.zero;
        foreach (var pos in range)
        {
            Vector2Int dir = heroPos - pos;
            int absX = Mathf.Abs(dir.x); int absY = Mathf.Abs(dir.y);
            if (absX == absY)
            {
                targetPos = pos;
                break;
            }
        }

        var path = TokenSystem.Instance.GetShortestPath(enemy, targetPos);
        if (path != null)
        {
            if (path.Count > 2)
                path.RemoveRange(2, path.Count - 2);
        }
        return path;
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

    public override void SetDrawMoveActionVG(bool active, EnemyView enemy, List<Vector2Int> path)
    {
        //일단 보류
        if (active)
        {
            VisualGridCreator.Instance.RemoveVisualGrid(enemy.gameObject.GetInstanceID(), "Enemy_Move");

            if (path == null) return;
            foreach (var pos in path)
            {
                VisualGridCreator.Instance.CreateVisualGrid(enemy.gameObject.GetInstanceID(), pos, "Enemy_Move");
            }
        }
        else
        {
            VisualGridCreator.Instance.RemoveVisualGrid(enemy.gameObject.GetInstanceID(), "Enemy_Move");
        }
    }

    public override void PlayMoveAction(EnemyView enemy, List<Vector2Int> path)
    {
        if (path == null) return;

        path = CheckHeroInPath(path);
        if (path.Count == 0) return;

        PerformMoveGA performMoveGA = new(enemy, path);
        ActionSystem.Instance.AddReaction(performMoveGA);
    }

    public override Enemy Clone()
    {
        return new ParasolMushroom();
    }
}
