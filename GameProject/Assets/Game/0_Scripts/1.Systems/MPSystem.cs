using System.Collections;
using UnityEngine;

public class MPSystem : Singleton<MPSystem>
{
    [SerializeField] private MPUI mpUI;
    public int MaxMP { get; private set; } = 3;
    public int CurrentMP { get; private set; }

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<SpendMPGA>(SpendMPPerformer);
        ActionSystem.AttachPerformer<RefillMPGA>(RefillMPPerformer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<SpendMPGA>();
        ActionSystem.DetachPerformer<RefillMPGA>();
    }
    public bool HasEnoughAP(int ap = 1)
    {
        return CurrentMP >= ap;
    }
    private IEnumerator SpendMPPerformer(SpendMPGA spendMPGA)
    {
        CurrentMP -= spendMPGA.Amount;

        mpUI.SpendMPUI(CurrentMP);
        yield return null;
    }
    private IEnumerator RefillMPPerformer(RefillMPGA refillMPGA)
    {
        CurrentMP = MaxMP;

        mpUI.RefillMPUI(CurrentMP);
        yield return null;
    }
}
