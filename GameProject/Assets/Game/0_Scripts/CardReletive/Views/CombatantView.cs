using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class CombatantView : Token
{
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private Transform wrapper;

    [SerializeField] private StatusEffectsUI statusEffectsUI;
    private Dictionary<StatusEffectType, int> statusEffectUIs = new();

    public int MaxHealth { get; private set; }
    public int CurrentHealth { get; private set; }
    public void SetUpBase(int health, TokenData tokenData, float rotationStep)
    {
        MaxHealth = CurrentHealth = health;     //몬스터 & 플레이어 체력 셋업
        UpdateHealthText();

        foreach (Transform child in wrapper)     //몬스터 & 플레이어 모델 셋업
            Destroy(child.gameObject);
        this.TokenData = tokenData;
        TokenModel = Instantiate(tokenData.TokenModel, transform.position, Quaternion.identity, wrapper);
        TokenModel.Rotate(rotationStep);
    }
    public void UpdateHealthText()
    {
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
    public void AddStatusEffect(StatusEffectType type, int stackCount)
    {
        if (statusEffectUIs.ContainsKey(type))
        {
            statusEffectUIs[type] += stackCount;
        }
        else
        {
            statusEffectUIs.Add(type, stackCount);
        }
        statusEffectsUI.UpdateStatusEffect(type, GetStatusEffectStacks(type));
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
}
