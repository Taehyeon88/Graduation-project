using System.Collections;
using UnityEngine;

public class ManaSystem : Singleton<ManaSystem>
{
    public int MaxMana { get; private set; } = 3;
    public int CurrentMana { get; private set; }

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<SpendManaGA>(SpendManaPerformer);
        ActionSystem.AttachPerformer<RefillManaGA>(RefillManaPerformer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<SpendManaGA>();
        ActionSystem.DetachPerformer<RefillManaGA>();
    }
    public bool HasEnoughMana(int mana = 1)
    {
        return CurrentMana >= mana;
    }
    private IEnumerator SpendManaPerformer(SpendManaGA spendManaGA)
    {
        CurrentMana -= spendManaGA.Amount;
        yield return null;
    }
    private IEnumerator RefillManaPerformer(RefillManaGA refillManaGA)
    {
        CurrentMana = MaxMana;
        yield return null;
    }
}
