using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUseCustomRangeVG
{
    public Action<int, List<Vector2Int>> GetCustomRangeVGEvent();
}
