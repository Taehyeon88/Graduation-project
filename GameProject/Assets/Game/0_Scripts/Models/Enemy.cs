using System;

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
}
