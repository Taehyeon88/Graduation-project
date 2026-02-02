using System;

[Serializable]
public abstract class Enemy
{
    public abstract GameAction JudgeActActions(EnemyView myEnemyView);   //몬스터 행동 패턴 판단 함수
    public abstract PerformMoveGA JudgeMoveAction(EnemyView myEnemyView);   //다음으로 할 행동 판단 함수
}
