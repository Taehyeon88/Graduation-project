using System.Collections.Generic;
using UnityEngine;

//풀숲: 올라선 용병에게 회피를 주고, 떠나면 이 발판이 준 회피를 회수 (발판별 사본이 부여 목록을 가짐)
public class BushEffect : TerrainEffect
{
    [SerializeField] private int dodgeStack = 1;

    private HashSet<CombatantView> granted = new();   //이 발판으로 회피를 얻은 용병

    public override void OnEnter(CombatantView unit)
    {
        granted.RemoveWhere(v => v == null);   //파괴된 용병 정리
        granted.Add(unit);
        ActionSystem.Instance.AddReaction(new AddStatusEffectGA(StatusEffectType.DODGE, dodgeStack, new List<CombatantView> { unit }, unit));
    }

    public override void OnExit(CombatantView unit)
    {
        if (granted.Remove(unit))
            unit.RemoveStatusEffect(StatusEffectType.DODGE, dodgeStack);
    }

    public override TerrainEffect Clone()
    {
        var clone = (BushEffect)base.Clone();
        clone.granted = new HashSet<CombatantView>();
        return clone;
    }
}
