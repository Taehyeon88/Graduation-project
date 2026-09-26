using System.Collections;
using UnityEngine;

public class MPSystem : Singleton<MPSystem>
{
    [SerializeField] private MPUI mpUI;
    public int MaxMP { get; private set; } = 3;
    public int CurrentMP { get; private set; }

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<SpendMPGA>(SpendMPGAPerformer);
        ActionSystem.AttachPerformer<RefillMPGA>(RefillMPGAPerformer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<SpendMPGA>();
        ActionSystem.DetachPerformer<RefillMPGA>();
    }
    public bool HasEnoughMP(int mp = 1)
    {
        return CurrentMP >= mp;
    }
    private IEnumerator SpendMPGAPerformer(SpendMPGA spendMPGA)
    {
        CurrentMP -= spendMPGA.Amount;

        mpUI.SpendMPUI(CurrentMP);
        yield return null;
    }
    private IEnumerator RefillMPGAPerformer(RefillMPGA refillMPGA)
    {
        CurrentMP = MaxMP;

        mpUI.RefillMPUI(CurrentMP);
        yield return null;
    }
}
