using System.Collections.Generic;
using UnityEngine;

//언덕: 이동 기준칸이 이 발판인 영웅은 바로 뒤를 제외한 모든 빈 칸으로 이동할 수 있고, 기본 이동 범위 밖으로 이동하면 공격력·속도를 영구히 얻음
public class HillEffect : TerrainEffect, IExtendedMoveTerrain
{
    [SerializeField] private int damageBonus = 1;
    [SerializeField] private int speedBonus = 1;

    public override void OnEnter(CombatantView unit) { }

    public override void OnExit(CombatantView unit) { }

    public bool AllowsExtendedMove(CombatantView unit, Vector2Int from, Vector2Int destination)
    {
        return destination != from - unit.FrontDirection;   //바로 뒤 한 칸만 제외
    }

    public IEnumerable<GameAction> GetExtendedMoveReactions(CombatantView unit)
    {
        yield return new ModifyStatGA(StatType.DAMAGE, damageBonus, new() { unit }, unit);
        yield return new ModifyStatGA(StatType.SPEED, speedBonus, new() { unit }, unit);
    }
}
