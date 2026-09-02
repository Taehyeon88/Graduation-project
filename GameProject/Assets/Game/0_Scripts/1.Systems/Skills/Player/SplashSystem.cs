using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplashSystem : MonoBehaviour
{
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<SplashGA>(SplashGAPerformer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<SplashGA>();
    }

    private IEnumerator SplashGAPerformer(SplashGA splashGA)
    {
        foreach (var targetPos in splashGA.TargetPoses)
        {
            CombatantView target = TokenSystem.Instance.API.GetTokenByPosition(targetPos) as CombatantView;
            var range = splashGA.GridRangeMode.GetGridRanges(targetPos, splashGA.Distance, splashGA.IsPentration);

            if (target != null)
            {
                DealDamageGA dealDamageGA = new(splashGA.Damage, new() { target }, splashGA.Caster);
                ActionSystem.Instance.AddReaction(dealDamageGA);

                List<CombatantView> splashTargets = Utility.PositionsToCombantViews(range, false, true);
                if (splashTargets.Count > 0)
                {
                    DealDamageGA dDGA = new(splashGA.SplashDamage, splashTargets, splashGA.Caster);
                    dealDamageGA.PostReactions.Add((dDGA, null));
                }
            }
            else
            {
                Debug.Log("해당 범위 안에 대상이 없음");
            }
        }

        yield return null;
    }
}
