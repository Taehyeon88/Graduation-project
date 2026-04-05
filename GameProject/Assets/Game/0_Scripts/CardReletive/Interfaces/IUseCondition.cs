using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUseCondition
{
    public bool[] IsMeetCondition(List<Vector2Int> targetPos);
}
