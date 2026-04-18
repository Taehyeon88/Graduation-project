using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class Enemy
{
    //Fuctions
    public abstract EnemyAction PreJudgeActAction(EnemyView enemy);                                          //다음 할 행동 미리 판단 (미리보기 포함)
    public abstract List<Vector2Int> PreJudgeMoveAction(EnemyView enemy);                                    //다음 이동 미리 판단 (미리보기 포함)
    public abstract void SetDrawActActionVG(bool active, EnemyView enemy, EnemyAction action);               //행동 미리보기 그리기 설정
    public abstract void SetDrawMoveActionVG(bool active, EnemyView enemy, List<Vector2Int> path);           //이동 미리보기 그리기 설정
    public abstract void PlayMoveAction(EnemyView enemy, List<Vector2Int> path);                             //이동 실행
    public abstract Enemy Clone();                                                                           //복사 함수


    //다른 적이 이동 할 위치인지 체크
    protected bool IsOtherEnemyWillMovePos(Vector2Int targetPos, EnemyView myEnemy)
    {
        var enemys = EnemySystem.Instance.Enemise;
        foreach (var enemy in enemys)
        {
            if (enemy == null) continue;
            if (enemy == myEnemy) break;

            var path = enemy.NextMovePath;
            if (path == null) continue;

            if (path[^1] == targetPos)
                return true;
        }
        return false;
    }

    /// <summary>
    /// 현재 EnemyView가 가지고 있는 EnemyAction을 가져오는 함수
    /// </summary>
    /// <param name="myEnemyView"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    protected EnemyAction FindEnemyAction(EnemyView myEnemyView, Type type)
    {
        foreach (var action in myEnemyView.Actions)
        {
            if(action.GetType() == type) return action;
        }
        return null;
    }

    protected List<Vector2Int> CheckHeroInPath(List<Vector2Int> path)
    {
        var heroPos = TokenSystem.Instance.GetTokenPosition(HeroSystem.Instance.HeroView);
        int index = path.IndexOf(heroPos);
        if (index >= 0)
        {
            path.RemoveRange(index, path.Count - index);
        }
        return path;
    }
}
