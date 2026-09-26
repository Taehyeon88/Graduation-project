using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldBashSystem : MonoBehaviour
{
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<ShieldBashGA>(ShieldBashGAPerformer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<ShieldBashGA>();
    }

    private IEnumerator ShieldBashGAPerformer(ShieldBashGA shieldBashGA)
    {
        List<IDamage> combatants = Utility.PositionsToIDamages(shieldBashGA.TargetPoses);

        if (combatants.Count > 0)
        {
            DealDamageGA dealDamageGA = new(shieldBashGA.Amount, combatants, shieldBashGA.myView);
            ActionSystem.Instance.AddReaction(dealDamageGA);

            int shieldStack = Mathf.CeilToInt(shieldBashGA.Amount);
            AddStatusEffectGA addStatusEffectGA = new(StatusEffectType.ARMOR, shieldStack, new() { shieldBashGA.myView });
            dealDamageGA.PostReactions.Add((addStatusEffectGA, null));
        }
        else
        {
            Debug.Log("해당 범위 안에 대상이 없음");
        }

        yield return null;
    }
}
