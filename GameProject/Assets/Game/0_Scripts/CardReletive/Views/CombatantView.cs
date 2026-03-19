using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using IsoTools;
using TMPro;
using System;
using UnityEngine;
using System.Linq;

public class CombatantView : Token
{
    [SerializeField] protected TMP_Text healthText;
    [SerializeField] private Transform wrapper;

    [SerializeField] private StatusEffectsUI statusEffectsUI;
    private Dictionary<StatusEffectType, int> statusEffectUIs = new();
    private StatusEffectStorage effectInfo = new();

    public int MaxHealth { get; private set; }
    public int CurrentHealth { get; private set; }
    public void SetUpBase(int health, TokenData tokenData, IsoObject isoObject)
    {
        MaxHealth = CurrentHealth = health;     //몬스터 & 플레이어 체력 셋업
        UpdateHealthText();

        foreach (Transform child in wrapper)    //몬스터 & 플레이어 모델 셋업
            Destroy(child.gameObject);
        this.TokenData = tokenData;
        TokenModel = Instantiate(tokenData.TokenModel, wrapper.transform.position, Quaternion.identity, wrapper);

        TokenTransform = isoObject;             //몬스터 & 플레이어 isomertric용 transform 셋업
    }
    public void UpdateHealthText()
    {
        if(healthText != null)
            healthText.text = $"HP : {CurrentHealth}";
    }
    public void Damage(int amount)
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
        transform.DOShakePosition(0.2f, 0.5f);
        UpdateHealthText();
    }
    public void AddStatusEffect(StatusEffectType type, int stackCount, Sprite sprite, float[] infoes = null)
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
    public void RemoveStatusEffect(StatusEffectType type, int stackCount)
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
    public StatusEffectType[] GetStatusEffects() => statusEffectUIs.Keys.ToArray();

    public StatusEffectStorage GetStatusEffectInfo(StatusEffectType type)
    {
        if (statusEffectUIs.ContainsKey(type)) return effectInfo;
        else return default;
    }
}
