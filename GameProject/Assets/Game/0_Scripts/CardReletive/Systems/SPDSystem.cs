using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SPDSystem : Singleton<SPDSystem>
{
    int maxSPD = 0;
    private int currentSPD = 0;
    public bool HasEnoughSPD(int amount)
    {
        return currentSPD >= amount;
    }
    public int RemainSPD() => currentSPD;
    public void AddSPD(int amount)
    {
        currentSPD = maxSPD = amount;
    }
    public void SpendSPD(int amount)
    {
        currentSPD -= amount;
    }
}
