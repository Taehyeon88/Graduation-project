using System.Collections;
using UnityEngine;

public class ManaSystem : Singleton<ManaSystem>
{
    public int MaxMana { get; private set; } = 5;
    public int CurrentMana { get; private set; }
    public int RefillAmount { get; private set; } = 3;

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<SpendManaGA>(SpendManaPerformer);
        ActionSystem.AttachPerformer<RefillManaGA>(RefillManaPerformer);
        ActionSystem.SubscribeReaction<EnemysTurnGA>(EnemysTurnPostReaction, ReactionTiming.POST);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<SpendManaGA>();
        ActionSystem.DetachPerformer<RefillManaGA>();
        ActionSystem.UnsubscribeReaction<EnemysTurnGA>(EnemysTurnPostReaction, ReactionTiming.POST);
    }
    public bool HasEnoughMana(int mana)
    {
        return CurrentMana >= mana;
    }
    public void ChangeMaxMana(int amount)
    {
        MaxMana = amount;
    }
    public void ChangeRefillAmount(int amount)
    {
        RefillAmount = amount;
    }
    private IEnumerator SpendManaPerformer(SpendManaGA spendManaGA)
    {
        CurrentMana -= spendManaGA.Amount;
        yield return null;
    }
    private IEnumerator RefillManaPerformer(RefillManaGA refillManaGA)
    {
        CurrentMana = Mathf.Min(MaxMana, CurrentMana + RefillAmount);
        yield return null;
    }

    //ReActions
    private void EnemysTurnPostReaction(EnemysTurnGA enemyTurnGA)
    {
        RefillManaGA refillManaGA = new();
        ActionSystem.Instance.AddReaction(refillManaGA);
    }
}
