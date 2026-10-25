using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using IsoTools;
using TMPro;
using System;
using UnityEngine;
using System.Linq;

public class CombatantView : Token, IDamageable
{
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text damageText;
    [SerializeField] private TMP_Text speedText;
    [SerializeField] private StatusEffectsUI statusEffectsUI;
    [SerializeField] private TraitsUI traitsUI;   //특성 UI가 없는 프리팹(장애물 등)은 비워 둔다

    protected Dictionary<StatusEffectType, int> statusEffectUIs = new();
    public int MaxHealth
    {
        get { return maxHealth; }
        private set
        {
            maxHealth = Mathf.Max(1, value);
            UpdateHealthUI();
        }
    }
    public int CurrentHealth
    {
        get { return currentHealth; }
        protected set
        {
            currentHealth = value;
            UpdateHealthUI();
        }
    }

    public const int MaxTraitCount = 2;   //유닛당 특성 최대 개수

    public int BaseDamage   //공격력 (IDamageable.Damage 메서드와 이름이 겹쳐 BaseDamage로 노출)
    {
        get { return baseDamage; }
        private set
        {
            baseDamage = Mathf.Max(1, value);
            UpdateDamageUI();
        }
    }
    public int Speed
    {
        get { return speed; }
        private set
        {
            speed = Mathf.Max(0, value);
            UpdateSpeedUI();
        }
    }
    public IReadOnlyList<Trait> Traits => traits;

    private int maxHealth;
    private int currentHealth;
    private int baseDamage;
    private int speed;
    private readonly List<Trait> traits = new();
    private readonly List<TraitData> activeTraitDatas = new();   //적용 중인 특성 데이터 (조회용)

    protected virtual void OnDisable()
    {
        foreach (var trait in traits)
        {
            trait.OnRemove();
        }
    }

    public void SetUpBase(CombatantData data)
    {
        SetUpBase(data, data.Health, data.Damage, data.Speed, data.Traits);
    }

    //영웅처럼 데이터 값 대신 개체별 유효 스탯·특성을 쓸 때
    public void SetUpBase(CombatantData data, int health, int damage, int speed, IReadOnlyList<TraitData> traitDatas)
    {
        MaxHealth = health;
        CurrentHealth = health;
        BaseDamage = damage;
        Speed = speed;
        SetUpBaseBase(data);
        SetUpTraits(data, traitDatas);
        if (traitsUI != null) traitsUI.SetUp(traits);
    }

    //특성 구독 시작 (최대 개수 초과 시 앞의 MaxTraitCount개만 사용)
    private void SetUpTraits(CombatantData data, IReadOnlyList<TraitData> traitDatas)
    {
        traits.Clear();
        activeTraitDatas.Clear();
        if (traitDatas == null) return;

        if (traitDatas.Count > MaxTraitCount)
            Debug.LogError($"{data.name}: 특성은 최대 {MaxTraitCount}개까지 가능합니다. (현재 {traitDatas.Count}개) 앞의 {MaxTraitCount}개만 사용합니다.");

        for (int i = 0; i < Mathf.Min(traitDatas.Count, MaxTraitCount); i++)
        {
            if (traitDatas[i] == null) continue;

            activeTraitDatas.Add(traitDatas[i]);

            if (traitDatas[i].Condition == null || traitDatas[i].Effect == null) continue;

            Trait trait = new(traitDatas[i]);
            trait.SetOwner(this);
            trait.OnAdd();
            traits.Add(trait);
        }
    }

    //적용 중인 특성 중 해당 Id가 있는지 (획득 특성 포함)
    public bool HasTrait(int traitId)
    {
        return activeTraitDatas.Any(t => t.Id == traitId);
    }

    //적용 중인 특성 중 해당 효과와 같은 클래스를 효과로 쓰는 특성의 Id (효과가 자기 특성을 찾을 때 사용)
    public bool TryGetTraitIdByEffect(TraitEffect effect, out int traitId)
    {
        traitId = 0;

        TraitData found = activeTraitDatas.FirstOrDefault(t => t.Effect != null && t.Effect.GetType() == effect.GetType());
        if (found == null) return false;

        traitId = found.Id;
        return true;
    }

    protected virtual Vector2Int Forward => Vector2Int.zero;   //정면 방향 (진영별 오버라이드)
    public Vector2Int FrontDirection => Forward;               //정면 방향 읽기 전용 (조건 등 외부 판정용)
    protected virtual bool IsTarget(Token token) => false;     //공격 대상 여부 (진영별 오버라이드)

    protected virtual int GetMoveLimitX(int line) => 0;        //해당 라인에서 이동 가능한 마지막 열 (상대 배치 위치 직전, 진영별 오버라이드)

    private bool IsAtMoveLimit(Vector2Int pos) => (GetMoveLimitX(pos.y) - pos.x) * Forward.x <= 0;

    //자동 전투 공용 패턴 (판단은 모두 여기서, 예약만 GA로)
    //1) 이동 한계 전이고 정면이 비어 있으면 1칸 이동 예약 2) 이동했다고 가정한 위치의 정면에 적이 있으면 공격, 이동 한계인데 정면이 비어 있으면 상대 코어 공격
    public virtual IEnumerator Battle()
    {
        if (Forward == Vector2Int.zero) yield break;

        //특성이 행동을 대체하면 기본 이동·공격은 하지 않음
        foreach (var trait in traits)
        {
            if (trait.TryOverrideBattle()) yield break;
        }

        TokenServiceAPI api = TokenSystem.Instance.API;
        Vector2Int pos = api.GetTokenPosition(this);
        Vector2Int front = pos + Forward;

        if (!IsAtMoveLimit(pos) && api.IsBound(front) && api.IsGridEmpty(front))
        {
            ActionSystem.Instance.AddReaction(new MoveGA(this, front));
            pos = front;
        }

        ReserveAttack(pos);
    }

    //pos 기준 정면 공격 예약 (정면에 적이 있으면 공격, 이동 한계인데 정면이 비어 있으면 상대 코어 공격)
    public void ReserveAttack(Vector2Int pos)
    {
        if (Forward == Vector2Int.zero) return;

        TokenServiceAPI api = TokenSystem.Instance.API;
        Vector2Int front = pos + Forward;

        Token frontToken = api.GetTokenByPosition(front);
        if (frontToken != null && IsTarget(frontToken) && frontToken is IDamageable target)
            ActionSystem.Instance.AddReaction(new DealDamageGA(BaseDamage, target, this, true));
        else if (IsAtMoveLimit(pos) && api.IsBound(front) && api.IsGridEmpty(front))
            ActionSystem.Instance.AddReaction(new AttackCoreGA(this));
    }

    private void UpdateHealthUI()
    {
        if (healthText != null)
            healthText.SetText(CurrentHealth.ToString());
    }

    private void UpdateDamageUI()
    {
        if (damageText != null)
            damageText.SetText(BaseDamage.ToString());
    }

    private void UpdateSpeedUI()
    {
        if (speedText != null)
            speedText.SetText(Speed.ToString());
    }

    public virtual void Damage(int amount, DealDamageGA dealDamageGA)
    {
        SoundSystem.Instance.PlaySound(1);
        Tween hit_Tween = Utility.GetModelShakeTween(
                this,
                0.18f,
                new Vector3(0.10f, 0.03f, 0f),
                10,
                90f
            );

        if (amount > 0)
        {
            CurrentHealth = Mathf.Max(CurrentHealth - amount, 0);
        }

        if(CurrentHealth <= 0)
        {
            hit_Tween.Pause();
            KillGA killGA = new KillGA(this, hit_Tween, dealDamageGA.Caster);
            dealDamageGA.PostReactions.Add((killGA, null));
        }
    }
    public void Heal(int amount)
    {
        CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
    }
    //스탯 증가 (체력은 최대·현재를 함께 증가, 음수는 사망 처리를 하지 않으므로 양수만 사용)
    public void ModifyStat(StatType type, int amount)
    {
        switch (type)
        {
            case StatType.HEALTH:
                MaxHealth += amount;
                CurrentHealth += amount;
                break;
            case StatType.DAMAGE:
                BaseDamage += amount;
                break;
            case StatType.SPEED:
                Speed += amount;
                break;
        }
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

    public void ReduceSEWhenMyTurnEnd()  //공용 디버프 삭제
    {
        RemoveStatusEffect(StatusEffectType.WEAK, 1);       //취약 감소
        RemoveStatusEffect(StatusEffectType.VULNERABLE, 1); //약화 감소
    }
}
