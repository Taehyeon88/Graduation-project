using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemyView : CombatantView
{
    [SerializeField] private TMP_Text attackText;
    public int AttackPower { get; set; }
    public void SetUp()
    {
        AttackPower = 5;
        UpdateAttckText();
        SetUpBase(AttackPower, null);   //임시(더미 데이터)
    }
    private void UpdateAttckText()
    {
        attackText.text = $"ATK : {AttackPower}";
    }
}
