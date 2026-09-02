using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackEnemyGA : GameAction
{
    public List<Vector2Int> TargetPoses { get; private set; } //공격 범위
    public float Amount { get; private set; }    //공격력
    public int Count { get; private set; }       //반복 횟수
    public HeroView MyView { get; private set; } //공격자
    public HeroAnimationType animationType { get; private set; }  //연출 타입

    public AttackEnemyGA(List<Vector2Int> targetPoses, float amount, int count, HeroView myView, HeroAnimationType animationType)
    {
        this.TargetPoses = targetPoses;
        this.Amount = amount;
        this.Count = count;
        this.MyView = myView;
        this.animationType = animationType;
    }
}
