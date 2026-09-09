using System.Collections;
using UnityEngine;

public class ShoulderBashSystem : MonoBehaviour
{
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<ShoulderBashGA>(ShoulderBashGAPerformer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<ShoulderBashGA>();
    }


    private IEnumerator ShoulderBashGAPerformer(ShoulderBashGA shoulderBashGA)
    {
        Vector2Int currentPos = TokenSystem.Instance.API.GetTokenPosition(shoulderBashGA.myView);
        Vector2Int targetPos = shoulderBashGA.TargetPoses[0];
        CombatantView heroView = shoulderBashGA.myView;

        var path = TokenSystem.Instance.API.GetShortestPath(heroView, targetPos);
        if (path != null)
        {
            PerformMoveGA performMoveGA = new(heroView, path);
            ActionSystem.Instance.AddReaction(performMoveGA);

            Vector2Int attackPos = path[0] + (path[0] - currentPos);
            CombatantView target = TokenSystem.Instance.API.GetTokenByPosition(attackPos) as CombatantView;

            if (target != null)
            {
                //연출


                //이동 이후, 공격 체인
                DealDamageGA dealDamageGA = new(shoulderBashGA.Damage, target, heroView);
                ActionSystem.Instance.AddReaction(dealDamageGA);

                //연출


                //공격 이후, 대상 넉백 체인
                KnockBackGA knockBackGA = new(heroView, shoulderBashGA.Distance, attackPos, (path[0] - currentPos));
                ActionSystem.Instance.AddReaction(knockBackGA);
            }
            else
            {
                Debug.Log("대상이 존재하지 않음");
            }
        }
        yield return null;
    }
}
