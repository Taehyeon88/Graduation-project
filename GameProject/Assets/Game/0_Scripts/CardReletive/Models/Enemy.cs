using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public abstract class Enemy
{
    //Fuctions
    public abstract EnemyAction PreJudgeActAction(EnemyView enemy);                                          //다음 할 행동 미리 판단 (미리보기 포함)
    public abstract void JudgeAndPlayMove(EnemyView enemy);                                                  //판단 후 다음 할 행동 실행
    public abstract void SetDrawActActionVG(bool active, EnemyView enemy, EnemyAction action);               //행동 미리보기 그리기 설정
    public abstract EnemyAction ReCalculate(EnemyView enemy);                                                //플레이어 턴중 재계산
    public abstract Enemy Clone();                                                                           //복사 함수

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

    protected List<Vector2Int> CheckTokenInPath(List<Vector2Int> path)
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
