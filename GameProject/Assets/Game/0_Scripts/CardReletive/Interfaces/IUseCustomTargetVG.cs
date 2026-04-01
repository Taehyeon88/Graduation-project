using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUseCustomTargetVG
{
    public Action<int> GetCustomTargetVGEvent(int ownerID);
}
