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
        if (healthSlider != null && healthText != null)
        {
            if (TurnSystem.Instance.CurrentTurn == TurnType.GameSetUp)
            {
                healthSlider.value = CurrentHealth / (float)MaxHealth;
            }
            else
            {
                float value = healthSlider.value;
                DOTween.To(
                    () => value,
                    x =>
                    {
                        healthSlider.value = value = x;
                    },
                    CurrentHealth / (float)MaxHealth,
                    0.12f);
            }
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

    public virtual void Damage(int amount, DealDamageGA dealDamageGA)
    {
        Tween hit_Tween = null;

        int remainingDamage = amount;
        int currentArmor = GetStatusEffectStacks(StatusEffectType.ARMOR);
        if (currentArmor > 0)
        {
            if (currentArmor >= remainingDamage)  //퍼펙트 방어
            {
                RemoveStatusEffect(StatusEffectType.ARMOR, remainingDamage);
                remainingDamage = 0;
                SoundSystem.Instance.PlaySound(3);
                hit_Tween = Utility.GetModelShakeTween(
                        this,
                        0.08f,
                        new Vector3(0.05f, 0.015f, 0f),
                        3,
                        0f
                    );
            }
            else if (currentArmor < remainingDamage)//방패 파괴
            {
                RemoveStatusEffect(StatusEffectType.ARMOR, currentArmor);
                remainingDamage -= currentArmor;
                SoundSystem.Instance.PlaySound(4);
                hit_Tween = Utility.GetModelShakeTween(
                        this,
                        0.14f,
                        new Vector3(0.07f, 0.02f, 0f),
                        6,
                        60f
                    );
            }
        }
        else  //방패 없음
        {
            SoundSystem.Instance.PlaySound(1);
            hit_Tween = Utility.GetModelShakeTween(
                    this,
                    0.18f,
                    new Vector3(0.10f, 0.03f, 0f),
                    10,
                    90f
                );
        }

        if (remainingDamage > 0)
        {
            CurrentHealth = Mathf.Max(CurrentHealth - remainingDamage, 0);
        }

        if(CurrentHealth <= 0)
        {
            hit_Tween.Pause();
            KillGA killGA = new KillGA(this, hit_Tween);
            dealDamageGA.PostReactions.Add((killGA, null));
        }
    }
    public void Heal(int amount)
    {
        CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
    }
    public virtual void AddStatusEffect(StatusEffectType type, int stackCount, Sprite sprite)
    {
        if (statusEffectUIs.ContainsKey(type))
        {
            statusEffectUIs[type] += stackCount;
        }
        else
        {
            statusEffectUIs.Add(type, stackCount);
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
}
