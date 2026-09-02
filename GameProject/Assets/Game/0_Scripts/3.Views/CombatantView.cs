using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using IsoTools;
using TMPro;
using System;
using UnityEngine;
using System.Linq;
using UnityEngine.UI; 

public class CombatantView : Token
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text movePointText;
    [SerializeField] private StatusEffectsUI statusEffectsUI;

    protected Dictionary<StatusEffectType, int> statusEffectUIs = new();
    private StatusEffectStorage effectInfo = new();
    public int MaxHealth
    {
        get { return maxHealth; }
        private set
        {
            maxHealth = value;
            UpdateHealthUI();
        }
    }
    public int CurrentHealth
    {
        get { return currentHealth; }
        private set
        {
            currentHealth = value;
            UpdateHealthUI();
        }
    }

    public int MovePoint { get; private set; }

    public int CurrentMovePoint
    {
        get { return currentMovePoint; }
        private set
        {
            currentMovePoint = value;
            UpdateMovePointUI();
        }
    }

    private int maxHealth;
    private int currentHealth;
    private int currentMovePoint;

    public void SetUpBase(int health, int maxHealth, int movePoint, TokenData tokenData, IsoObject isoObject)
    {
        CurrentHealth = health;
        MaxHealth = maxHealth;
        CurrentMovePoint = MovePoint = movePoint;
        SetUpBaseBase(tokenData, isoObject);
    }

    private void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = MaxHealth;
            healthSlider.value = CurrentHealth;
        }
        if (healthText != null)
        {
            healthText.SetText($"{CurrentHealth}/{MaxHealth}");
        }
    }

    private void UpdateMovePointUI()
    {
        if (movePointText != null)
        {
            movePointText.SetText(CurrentMovePoint.ToString());
        }
    }

    public void ResetMovePoint()
    {
        CurrentMovePoint = MovePoint;
    }

    public bool HasEnoughMovePoint(int movePoint)
    {
        return CurrentMovePoint >= movePoint;
    }

    public void SpendMovePoint(int movePoint)
    {
        CurrentMovePoint -= movePoint;
    }

    public virtual void Damage(int amount)
    {
        int remainingDamage = amount;
        int currentArmor = GetStatusEffectStacks(StatusEffectType.ARMOR);
        if (currentArmor > 0)
        {
            if (currentArmor >= remainingDamage)
            {
                RemoveStatusEffect(StatusEffectType.ARMOR, remainingDamage);
                remainingDamage = 0;
            }
            else if (currentArmor < remainingDamage)
            {
                RemoveStatusEffect(StatusEffectType.ARMOR, currentArmor);
                remainingDamage -= currentArmor;
            }
        }
        if (remainingDamage > 0)
        {
            CurrentHealth = Mathf.Max(CurrentHealth - remainingDamage, 0);
        }

        if (CurrentHealth > 0)
            transform.DOShakePosition(0.2f, 0.5f);
    }
    public void Heal(int amount)
    {
        CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
    }
    public virtual void AddStatusEffect(StatusEffectType type, int stackCount, Sprite sprite, float[] infoes = null)
    {
        if (statusEffectUIs.ContainsKey(type))
        {
            statusEffectUIs[type] += stackCount;
        }
        else
        {
            statusEffectUIs.Add(type, stackCount);
            effectInfo.SetStatusEffectInfo(infoes, type);    //해당 StatusEffectInfo 저장
        }
        statusEffectsUI.UpdateStatusEffect(type, GetStatusEffectStacks(type), sprite);
    }
    public virtual void RemoveStatusEffect(StatusEffectType type, int stackCount)
    {
        if (statusEffectUIs.ContainsKey(type))
        {
            statusEffectUIs[type] -= stackCount;
            if (statusEffectUIs[type] <= 0)
            {
                statusEffectUIs.Remove(type);
            }
            statusEffectsUI.UpdateStatusEffect(type, GetStatusEffectStacks(type));
        }
    }
    public int GetStatusEffectStacks(StatusEffectType type)
    {
        if(statusEffectUIs.ContainsKey(type)) return statusEffectUIs[type];
        else return 0;
    }
    public bool CheckStatusEffectExist(StatusEffectType type) => statusEffectUIs.ContainsKey(type);
    public StatusEffectType[] GetStatusEffects() => statusEffectUIs.Keys.ToArray();

    public StatusEffectStorage GetStatusEffectInfo(StatusEffectType type)
    {
        if (statusEffectUIs.ContainsKey(type)) return effectInfo;
        else return default;
    }
}
