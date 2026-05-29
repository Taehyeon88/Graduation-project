using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPDSystem : Singleton<SPDSystem>
{
    public int currentSPD { get; private set; }


    private void OnEnable()
    {
        ActionSystem.AttachPerformer<AddSpdGA>(AddSpdGAPerformer);
        ActionSystem.AttachPerformer<ResetSpdGA>(ResetSpdGAPerformer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<AddSpdGA>();
        ActionSystem.DetachPerformer<ResetSpdGA>();
    }

    private IEnumerator AddSpdGAPerformer(AddSpdGA addSpdGA)
    {
        currentSPD += addSpdGA.Amount;
        yield return null;
    }
    private IEnumerator ResetSpdGAPerformer(ResetSpdGA resetSpdGA)
    {
        currentSPD = 0;
        yield return null;
    }

    public void SpendSPD(int amount)
    {
        currentSPD -= amount;
    }
}
