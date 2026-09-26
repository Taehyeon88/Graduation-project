using System.Collections;
using UnityEngine;

public class APSystem : Singleton<APSystem>
{
    [SerializeField] private APUI apUI;
    public int MaxAP { get; private set; } = 3;
    public int CurrentAP { get; private set; }

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<SpendAPGA>(SpendAPPerformer);
        ActionSystem.AttachPerformer<RefillAPGA>(RefillAPPerformer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<SpendAPGA>();
        ActionSystem.DetachPerformer<RefillAPGA>();
    }
    public bool HasEnoughAP(int ap = 1)
    {
        return CurrentAP >= ap;
    }
    private IEnumerator SpendAPPerformer(SpendAPGA spendAPGA)
    {
        CurrentAP -= spendAPGA.Amount;

        apUI.SpendAPUI(CurrentAP);
        yield return null;
    }
    private IEnumerator RefillAPPerformer(RefillAPGA refillManaGA)
    {
        CurrentAP = MaxAP;

        apUI.RefillAPUI(CurrentAP);
        yield return null;
    }
}
