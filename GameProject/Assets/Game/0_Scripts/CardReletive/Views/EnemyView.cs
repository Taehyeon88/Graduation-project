using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemyView : CombatantView
{
    [SerializeField] private TMP_Text attackText;
    public int AttackPower { get; set; }
    public void SetUp(EnemyData enemyData, float rotationStep)
    {
        AttackPower = enemyData.AttackPower;
        UpdateAttckText();
        SetUpBase(enemyData.Health, enemyData, rotationStep);
    }
    private void UpdateAttckText()
    {
        attackText.text = $"ATK : {AttackPower}";
    }
}
