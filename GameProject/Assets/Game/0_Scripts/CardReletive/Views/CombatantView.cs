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
    [SerializeField] private Transform wrapper;
    [SerializeField] private Transform genTransform;

    protected Dictionary<StatusEffectType, int> statusEffectUIs = new();
    private StatusEffectStorage effectInfo = new();
    public int MaxHealth
    {
        get { return maxHealth; }
        private set
        {
            maxHealth = value;
            UpdateHealth();
        }
    }
    public int CurrentHealth
    {
        get { return currentHealth; }
        private set
        {
            currentHealth = value;
            UpdateHealth();
        }
    }

    private int maxHealth;
    private int currentHealth;

    public void SetUpBase(int health, TokenData tokenData, IsoObject isoObject)
    {
        MaxHealth = CurrentHealth = health;     //몬스터 & 플레이어 체력 셋업

        foreach (Transform child in wrapper)    //몬스터 & 플레이어 모델 셋업
            Destroy(child.gameObject);
        this.TokenData = tokenData;
        TokenModel = Instantiate(tokenData.TokenModel, genTransform? genTransform.position : wrapper.position, Quaternion.identity, wrapper);

        TokenTransform = isoObject;             //몬스터 & 플레이어 isomertric용 transform 셋업
    }

    public void UpdateHealth()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = MaxHealth;
            healthSlider.value = CurrentHealth;
        }
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
        UISystem.Instance.UpdateStatusEffectUI(this, type, GetStatusEffectStacks(type), sprite);
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
            UISystem.Instance.UpdateStatusEffectUI(this, type, GetStatusEffectStacks(type));
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

    public void Cheat_GetHealth(int amount)
    {
        CurrentHealth = MaxHealth = amount;
    }
}
