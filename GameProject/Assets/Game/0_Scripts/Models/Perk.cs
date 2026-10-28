using System;
using UnityEngine;

public class Perk
{
    public Sprite Image => data.Image;
    public string Description => data.Description;
    public string Title { get; private set; }
    public bool HasLimit => data.MaxTriggersPerBattle > 0;   //전투당 발동 상한 유무
    public int RemainingTriggers => Mathf.Max(0, data.MaxTriggersPerBattle - triggerCount);   //남은 발동 횟수 (HasLimit일 때만 의미 있음)
    public bool IsExhausted => HasLimit && triggerCount >= data.MaxTriggersPerBattle;

    public event Action OnCountChanged;   //발동 횟수가 바뀔 때 (UI 갱신용)

    private readonly PerkData data;
    private readonly PerkCondition condition;
    private readonly PerkEffect effect;
    private int triggerCount;   //이번 전투의 발동 횟수 (전투 시작 때 ResetBattleCount로 초기화)

    public Perk(PerkData perkData)
    {
        data = perkData;
        condition = perkData.Condition.Clone();   //SO 공유를 피하려고 사본을 보유
        effect = perkData.Effect.Clone();

        string name = data.name;
        int index = name.IndexOf("_");
        Title = index >= 0 ? name.Substring(index + 1) : name;
    }
    public void OnAdd()
    {
        condition.SubscribeCondition(Reaction);
    }
    public void OnRemove()
    {
        condition.UnsubscribeCondition(Reaction);
    }
    public void ResetBattleCount()
    {
        triggerCount = 0;
        OnCountChanged?.Invoke();
    }
    public void Reaction(GameAction gameAction)
    {
        if (!condition.SubConditionIsMat(gameAction)) return;
        if (IsExhausted) return;

        triggerCount++;
        OnCountChanged?.Invoke();
        effect.PerformReaction(gameAction);
    }
}
