using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackEnemySystem : MonoBehaviour
{
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<AttackEnemyGA>(AttackEnemyGAPerformer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<AttackEnemyGA>();
    }

    private IEnumerator AttackEnemyGAPerformer(AttackEnemyGA attackEnemyGA)
    {
        //공격 연출


        //공격 로직
        var targets = Utility.PositionsToCombantViews(attackEnemyGA.TargetPoses);
        if (targets.Count > 0)
        {
            DealDamageGA dealDamageGA = new(attackEnemyGA.Amount, targets, attackEnemyGA.MyView);
            ActionSystem.Instance.AddReaction(dealDamageGA);
        }
        yield return null;
    }
}
