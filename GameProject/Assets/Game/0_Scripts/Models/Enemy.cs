using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
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
    /// <param name="enemy"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    protected EnemyAction FindEnemyAction(EnemyView enemy, Type type)
    {
        foreach (var action in enemy.Actions)
        {
            if(action.GetType() == type) return action;
        }
        return null;
    }

    protected bool ChangeToWaitEA(EnemyView enemy, EnemyAction enemyAction, Vector2Int currentPosition)
    {
        //행동 범위 안에 플레이어 존재 여부 확인
        //존재 하지 않을 경우, 대기 상태 설정 및 반환 처리

        EnemyRangeMode enemyRM = enemyAction.EnemyRM;
        int distance = enemyAction.ActDistance;
        var range = enemyRM.GetGridRanges(currentPosition, distance);

        if (!range.Contains(HeroSystem.Instance.HeroPosition))
        {
            //대기 상태로 변경
            WaitEA waitEA = FindEnemyAction(enemy, typeof(WaitEA)) as WaitEA;
            waitEA.ReservedEA = enemyAction;
            enemy.SetNextAction(waitEA);

            return true;
        }
        else return false;
    }
}
