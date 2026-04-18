using System;
using System.Collections.Generic;
using UnityEngine;

public class Hermit : Enemy
{
    public override EnemyAction PreJudgeActAction(EnemyView enemy)
    {
        var heroPos = TokenSystem.Instance.GetTokenPosition(HeroSystem.Instance.HeroView);
        var distance = TokenSystem.Instance.GetDistance(enemy, heroPos);

        Type type = null;
        if (distance <= 2)
        {
            type = typeof(Hermit_QxidizedEA);
        }
        else
        {
            type = typeof(NormalAttackEA);
        }

        var action = FindEnemyAction(enemy, type);
        if (action == null)
            Debug.LogError($"{this}에 {type}라는 행동이 존재하지 않습니다.");

        Vector2Int dir = TokenSystem.Instance.GetDirection(HeroSystem.Instance.HeroView, enemy);
        action.Directions.Add(dir);
        return action;
    }

    public override List<Vector2Int> PreJudgeMoveAction(EnemyView enemy)
    {
        //인접 2 칸 내 플레이어가 존재할 시: 플레이어로부터 1칸 도망
        //인접 2칸 내 플레이어가 없을 시: 이동 X

        var heroPos = TokenSystem.Instance.GetTokenPosition(HeroSystem.Instance.HeroView);
        var distance = TokenSystem.Instance.GetDistance(enemy, heroPos);

        if (distance <= 2)
        {
            var range = TokenSystem.Instance.GetCanMovePlace(enemy, 1);
            int maxDistance = 0;
            Vector2Int position = Vector2Int.zero;
            foreach (var pos in range)
            {
                int dis = TokenSystem.Instance.GetDistance(HeroSystem.Instance.HeroView, pos);
                if (dis > maxDistance)
                {
                    maxDistance = dis;
                    position = pos;
                }
            }
            return new() { position };
        }
        else
        {
            return null;
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
        return new Hermit();
    }
}
