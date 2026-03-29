using System.Collections;
using System.Collections.Generic;
using IsoTools;
using UnityEngine;

public class AoECreator : Singleton<AoECreator>
{
    public IsoObject CreateAoE(AoEModel model)
    {
        var aoE = Instantiate(model);
        return aoE.GetComponent<IsoObject>();
    }
}
