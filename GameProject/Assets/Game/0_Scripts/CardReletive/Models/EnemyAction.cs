using UnityEngine;

[System.Serializable]
public abstract class EnemyAction
{
    //행동 대상(범위) 설정용
    public EnemyRangeMode EnemyRM { get; set; }
    public int ActDistance { get; set; }
    public bool IsPenetration { get; set; }

    //다음에 할 행동 표시용
    public abstract Sprite Icon { get; protected set; }
    public abstract string Description { get; protected set; }

    public abstract void PlayEnemyAction(EnemyView enemy);
    public abstract EnemyAction Clone();  //복사 함수
}
