using System.Collections.Generic;
using UnityEngine;

//이동 기준칸이 이 발판인 용병의 이동 범위를 넓히는 발판 효과 (MoveSystem이 이동 범위 계산 때 질의)
public interface IExtendedMoveTerrain
{
    public bool AllowsExtendedMove(CombatantView unit, Vector2Int from, Vector2Int destination);   //기본 이동 범위 밖의 destination으로 이동 가능한지

    public IEnumerable<GameAction> GetExtendedMoveReactions(CombatantView unit);                  //넓어진 범위로 이동한 뒤 이어서 실행할 GA
}
