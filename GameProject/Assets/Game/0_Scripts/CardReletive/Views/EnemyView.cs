using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemyView : CombatantView
{
    [SerializeField] private TMP_Text attackText;
    public int AttackPower { get; set; }
    public Enemy Enemy { get; private set; }
    public GameAction actAction { get; set; }       //행동GameAction
    public PerformMoveGA moveAction { get; set; }   //이동GameAction
    public void SetUp(EnemyData enemyData, float rotationStep)
    {
        AttackPower = enemyData.AttackPower;
        Enemy = enemyData.Enemy;
        UpdateAttckText();
        SetUpBase(enemyData.Health, enemyData, rotationStep);
    }
    private void UpdateAttckText()
    {
        attackText.text = $"ATK : {AttackPower}";
    }
}
