using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMoveGA : GameAction
{
    public int Distance { get; private set; }
    public bool IsFirstMove { get; private set; }
    public bool IsAutoMove { get; private set; }
    public List<Vector2Int> TargetPoses { get; private set; }

    //해당 Performer에서 직접 다음 이동할 그리드 인터렉션을 할 경우.
    public PlayerMoveGA(int distance, bool isFirstMove = false)
    {
        this.Distance = distance;
        this.IsFirstMove = isFirstMove;
        this.IsAutoMove = false; ;
    }

    //이미 정해진 위치로 자동 이동을 사용할 경우.
    public PlayerMoveGA(int distance, List<Vector2Int> targetPoses)
    {
        this.Distance = distance;
        this.TargetPoses = targetPoses;
        this.IsFirstMove = false;
        this.IsAutoMove = true;
    }
}
