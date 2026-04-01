using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUseCustomTargetVG
{
    public Action<bool, int, List<Vector2Int>, Card> GetCustomTargetVGEvent();
}
