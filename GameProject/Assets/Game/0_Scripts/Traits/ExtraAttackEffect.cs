using UnityEngine;

//추가 공격 차례를 얻고 자신의 속도를 소모 (속도가 speedCost 미만이면 발동 안 함)
//권장 reactionTiming: POST (조건 AttackedOpponentCondition과 함께 — 추가 공격이 조건을 다시 충족해 속도가 speedCost 미만이 될 때까지 반복)
public class ExtraAttackEffect : TraitEffect
{
    [SerializeField] private int speedCost = 2;   //발동당 속도 소모량

    public override void PerformReaction(GameAction action, CombatantView owner)
    {
        if (owner.Speed < speedCost) return;

        ActionSystem.Instance.AddReaction(new ModifyStatGA(StatType.SPEED, -speedCost, new() { owner }, owner));
        ActionSystem.Instance.AddReaction(new ExtraAttackGA(owner));
    }
}
