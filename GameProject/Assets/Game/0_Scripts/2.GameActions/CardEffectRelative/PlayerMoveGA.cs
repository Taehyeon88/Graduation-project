using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveGA : GameAction
{
    public int Distance { get; private set; }
    public List<Vector2Int> TargetPoses { get; private set; }

    //해당 Performer에서 직접 다음 이동할 그리드 인터렉션을 할 경우.
    public PlayerMoveGA(int distance)
    {
        this.Distance = distance;
    }
}
