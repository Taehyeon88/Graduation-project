using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPDSystem : Singleton<SPDSystem>
{
    [SerializeField] private SPDUI spdUI;
    int maxSPD = 0;
    int currentSPD = 0;
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<AddSPDGA>(AddSPDPerformer);
        ActionSystem.AttachPerformer<SpendSPDGA>(SpendSPDPerformer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<AddSPDGA>();
        ActionSystem.DetachPerformer<SpendSPDGA>();
    }
    public bool HasEnoughSPD(int amount)
    {
        return currentSPD >= amount;
    }
    private IEnumerator AddSPDPerformer(AddSPDGA addSPDGA)
    {
        currentSPD = maxSPD = addSPDGA.Amount;
        spdUI.UpdateSPDUI(currentSPD, maxSPD);
        yield return null;
    }
    private IEnumerator SpendSPDPerformer(SpendSPDGA spendSPDGA)
    {
        currentSPD -= spendSPDGA.Amount;
        spdUI.UpdateSPDUI(currentSPD, maxSPD);
        yield return null;
    }
}
