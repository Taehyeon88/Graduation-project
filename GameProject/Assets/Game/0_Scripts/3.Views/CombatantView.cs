using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using IsoTools;
using TMPro;
using System;
using UnityEngine;
using System.Linq;
using UnityEngine.UI; 

public class CombatantView : Token, IDamageable
{
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text healthText;
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

    public const int MaxPerkCount = 2;   //유닛당 특성 최대 개수

    public int BaseDamage { get; private set; }   //공격력 (IDamageable.Damage 메서드와 이름이 겹쳐 BaseDamage로 노출)
    public int Speed { get; private set; }
    public IReadOnlyList<PerkItem> Perks => perks;

    private int maxHealth;
    private int currentHealth;
    private readonly List<PerkItem> perks = new();

    private void OnDisable()
    {
        foreach (var perk in perks)
        {
            perk.OnRemove();
        }
    }

    public void SetUpBase(CombatantData data)
    {
        CurrentHealth = data.Health;
        MaxHealth = data.Health;
        BaseDamage = data.Damage;
        Speed = data.Speed;
        SetUpBaseBase(data);
        SetUpPerks(data);
    }

    //특성 구독 시작 (최대 개수 초과 시 앞의 MaxPerkCount개만 사용)
    private void SetUpPerks(CombatantData data)
    {
        perks.Clear();
        if (data.Perks == null) return;

        if (data.Perks.Count > MaxPerkCount)
            Debug.LogError($"{data.name}: 특성은 최대 {MaxPerkCount}개까지 가능합니다. (현재 {data.Perks.Count}개) 앞의 {MaxPerkCount}개만 사용합니다.");

        for (int i = 0; i < Mathf.Min(data.Perks.Count, MaxPerkCount); i++)
        {
            if (data.Perks[i] == null) continue;

            PerkItem perk = new(data.Perks[i]);
            perk.SetOwner(this);
            perk.OnAdd();
            perks.Add(perk);
        }
    }

    protected virtual Vector2Int Forward => Vector2Int.zero;   //정면 방향 (진영별 오버라이드)
    protected virtual bool IsTarget(Token token) => false;     //공격 대상 여부 (진영별 오버라이드)

    //자동 전투 공용 패턴: 정면이 비어 있으면 1칸 이동 예약 → 공격 예약 (대상은 공격 실행 시점의 정면에서 결정)
    public virtual IEnumerator Battle()
    {
        if (Forward == Vector2Int.zero) yield break;

        TokenServiceAPI api = TokenSystem.Instance.API;
        Vector2Int front = api.GetTokenPosition(this) + Forward;
        if (api.IsBound(front) && api.IsGridEmpty(front))
            ActionSystem.Instance.AddReaction(new MoveGA(this, front));

        ActionSystem.Instance.AddReaction(new DealDamageGA(BaseDamage, this));
    }

    //현재 위치 기준 정면의 공격 대상 (없으면 null)
    public IDamageable GetFrontTarget()
    {
        TokenServiceAPI api = TokenSystem.Instance.API;
        Token frontToken = api.GetTokenByPosition(api.GetTokenPosition(this) + Forward);
        if (frontToken != null && IsTarget(frontToken) && frontToken is IDamageable target)
            return target;
        return null;
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

    public void ReduceSEWhenMyTurnStart()  //공용 버프 삭제
    {
        //방어 스택 삭제
        int armorStack = GetStatusEffectStacks(StatusEffectType.ARMOR);
        if (armorStack > 0) RemoveStatusEffect(StatusEffectType.ARMOR, armorStack);
    }
    public void ReduceSEWhenMyTurnEnd()  //공용 디버프 삭제
    {
        RemoveStatusEffect(StatusEffectType.WEAK, 1);       //취약 감소
        RemoveStatusEffect(StatusEffectType.VULNERABLE, 1); //약화 감소
    }
}
