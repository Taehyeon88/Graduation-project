using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Slime : Enemy
{
    private const int moveDistance = 1;    //이동 거리
    private const int attackDistance = 1;  //공격 사거리

    public override EnemyAction PreJudgeActAction(EnemyView myEnemyView)
    {
        Type type = typeof(Slime_AttackEA);
        var action = FindEnemyAction(myEnemyView, type);
        if (action == null)
            Debug.LogError($"{this}에 {type}라는 행동이 존재하지 않습니다.");

        action.EnemyRM = new E_AllAroundRM();
        action.ActDistance = attackDistance;
        action.IsPenetration = false;

        return action;
    }

    public override List<Vector2Int> PreJudgeMoveAction(EnemyView myEnemyView)
    {
        //규칙 : 플레이어 주변 타일로 목표로 1칸 이동

        //영웅 주변으로 이동 가능 타일 찾기
        var canMovePoses = TokenSystem.Instance.GetCanMovePlace(HeroSystem.Instance.HeroView, attackDistance);

        (int, List<Vector2Int>) minValues = GetMinValues(canMovePoses, myEnemyView);  //최소 경로의 목표 위치 찾기
        int distance = minValues.Item1;
        List<Vector2Int> path = minValues.Item2;

        if (distance >= 1 && distance != int.MaxValue)        //이동할 경로를 찾을 수 없음 - int.MaxValue
        {
            path.RemoveRange(1, path.Count - 1);
            return path;
        }
        else return null;
    }

    public override void SetDrawActActionVG(bool active, EnemyView enemy, EnemyAction enemyAction)
    {
        if (active)
        {
            VisualGridCreator.Instance.RemoveVisualGrid(enemy.gameObject.GetInstanceID(), "Enemy_Attack");

            var enemyRM = enemyAction.EnemyRM;
            int distance = enemyAction.ActDistance;
            bool isPenetration = enemyAction.IsPenetration;

            var poses = enemyRM.GetGridRanges(TokenSystem.Instance.GetTokenPosition(enemy), distance, isPenetration);
            foreach (var pos in poses)
            {
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
        path = CheckHeroInPath(path);
        if (path.Count == 0) return;

        PerformMoveGA performMoveGA = new(enemy, path);
        ActionSystem.Instance.AddReaction(performMoveGA);
    }


    //Privates
    private (int, List<Vector2Int>) GetMinValues(List<Vector2Int> canMovePoses, EnemyView myEnemyView)
    {
        //해당 타일들 중, 가장 거리가 가까운 타일 선정
        int minDistance = int.MaxValue;
        List<Vector2Int> targetPath = null;
        foreach (var pos in canMovePoses)
        {
            var path = TokenSystem.Instance.GetShortestPath(myEnemyView, pos);   //최소 경로 찾기
            if (path == null) continue;
            if (IsOtherEnemyWillMovePos(path[^1], myEnemyView)) continue;

            int dis = path.Count;
            if (dis < minDistance)
            {
                minDistance = dis;
                targetPath = path;
            }
            else if (dis == minDistance)  //같은 거리일 경우, 50퍼센트의 확률로 바뀜
            {
                float randomValue = UnityEngine.Random.value;
                if (randomValue > 0.5f)
                {
                    targetPath = path;
                }
            }
        }

        return (minDistance, targetPath);
    }

    public override Enemy Clone()
    {
        return new Slime();
    }
}
