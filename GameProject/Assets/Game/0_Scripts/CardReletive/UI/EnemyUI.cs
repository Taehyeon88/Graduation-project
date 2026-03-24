using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyUI : MonoBehaviour
{
    [SerializeField] private Image image;
    public EnemyView enemyView { get; private set; }
    private CombatantViewStatusUI combatantViewStatusUI;

    public void Set(EnemyView enemyView, CombatantViewStatusUI combatantViewStatusUI)
    {
        this.image.sprite = enemyView.EnemySprite;
        this.enemyView = enemyView;
        this.combatantViewStatusUI = combatantViewStatusUI;
    }

    public void Selected()
    {
        combatantViewStatusUI.SetEnemyUIInfos(enemyView);
    }
}
