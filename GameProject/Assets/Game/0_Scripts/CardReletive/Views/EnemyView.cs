using System.Collections;
using System.Collections.Generic;
using IsoTools;
using TMPro;
using UnityEngine;

public class EnemyView : CombatantView
{
    public string EnemyName { get; private set; }     //적 이름
    public Sprite EnemySprite { get; private set; }   //적 이미지
    public int AttackPower { get; set; }              //적 공격력 !추후수정
    public Enemy Enemy { get; private set; }          //적 모델
    public GameAction actAction { get; set; }         //다음 할 행동 GameAction
    public PerformMoveGA moveAction { get; set; }     //이동GameAction
    public void SetUp(EnemyData enemyData)
    {
        //Isometric 설정
        IsoObject isObject = GetComponent<IsoObject>();
        if (isObject == null)
            isObject = gameObject.AddComponent<IsoObject>();

        //Enemy 데이터 설정
        EnemyName = enemyData.name;
        EnemySprite = enemyData.TokenModel.Sprite;
        AttackPower = enemyData.AttackPower;
        Enemy = enemyData.Enemy;
        SetUpBase(enemyData.Health, enemyData, isObject);
    }
}
