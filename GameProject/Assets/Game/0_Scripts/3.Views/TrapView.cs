using IsoTools;
using UnityEngine;
using UnityEngine.UIElements;

public class TrapView : Token
{
    private TrapData trapData;
    private Trap trap;
    public void SetUp(TrapData trapData)
    {
        IsoObject isObject = GetComponent<IsoObject>();
        if (isObject == null)
            isObject = gameObject.AddComponent<IsoObject>();

        this.trapData = trapData;
        SetUpBaseBase(trapData, isObject);

        trap = trapData.Trap.Clone();
        trap.AddChain(this);
    }

    private void OnDisable()
    {
        trap.RemoveChain();
    }
}
