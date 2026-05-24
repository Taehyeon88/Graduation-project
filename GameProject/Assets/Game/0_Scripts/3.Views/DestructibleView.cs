using IsoTools;
using UnityEngine;

public class DestructibleView : CombatantView
{
    public void SetUp(TokenData tokenData)
    {
        IsoObject isObject = GetComponent<IsoObject>();
        if (isObject == null)
            isObject = gameObject.AddComponent<IsoObject>();

        SetUpBase(tokenData.Health, tokenData, isObject);
    }
}
