using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class Enemy
{
    public abstract EnemyActionInfo PreJudgeActAction(EnemyView myEnemyView);                                      //다음 할 행동 미리 판단 (미리보기 포함)
    public abstract List<Vector2Int> PreJudgeMoveAction(EnemyView myEnemyView);                                    //다음 이동 미리 판단 (미리보기 포함)
    public abstract void SetDrawActActionVG(bool active, EnemyView myEnemyView, EnemyActionInfo enemyActionInfo);  //행동 미리보기 그리기 설정
    public abstract void SetDrawMoveActionVG(bool active, EnemyView myEnemyView, List<Vector2Int> path);           //이동 미리보기 그리기 설정
    public abstract void PlayActAction(EnemyView myEnemyView, EnemyActionInfo enemyActionInfo);                    //행동 실행
    public abstract void PlayMoveAction(EnemyView myEnemyView, List<Vector2Int> path);                             //이동 실행


    //다른 적이 이동 할 위치인지 체크
    protected bool IsOtherEnemyWillMovePos(Vector2Int targetPos, EnemyView myEnemy)
    {
        var enemys = EnemySystem.Instance.Enemise;
        foreach (var enemy in enemys)
        {
            if (enemy == null) continue;
            if (enemy == myEnemy) break;

            var path = enemy.ActionInfo.movePath;
            if (path == null) continue;

            if (path[^1] == targetPos)
                return true;
        }
        return false;
    }
}
