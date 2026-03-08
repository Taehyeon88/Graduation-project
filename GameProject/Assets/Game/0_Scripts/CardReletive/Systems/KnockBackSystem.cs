using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class KnockBackSystem : MonoBehaviour
{
    private const int crachDamage = 3;
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<KnockBackGA>(KnockBackGAPerformer);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<KnockBackGA>();
    }

    //Performer
    private IEnumerator KnockBackGAPerformer(KnockBackGA knockBackGA)
    {
        int pushedDis = 0;               //실제 넉백 밀리는 거리
        bool chrash = false;

        Debug.Log("넉백 성공!!");

        ////넉백될 대상 찾기
        Token target = TokenSystem.Instance.GetTokenByPosition(knockBackGA.TargetPos);
        CombatantView chrashedTarget = null;

        //넉백 효과 판단 처리
        for (int d = 1; d <= knockBackGA.Distance; d++)
        {
            //밀려날 위치 받기
            Vector2Int pushedPos = knockBackGA.TargetPos + knockBackGA.Direction * d;

            if (!TokenSystem.Instance.IsBound(pushedPos)) chrash = true;
            else if (TokenSystem.Instance.GetTokenByPosition(pushedPos) != null) chrash = true;

            Debug.Log($"정보 - 밀린 위치 : {pushedPos}, 충돌 여부: {chrash}");

            if (!chrash) pushedDis = d;
            else
            {
                chrashedTarget = TokenSystem.Instance.GetTokenByPosition(pushedPos) as CombatantView;
                break;
            }
        }

        //넉백 애니메이션
        if (pushedDis > 0)
        {
            Debug.Log("밀리는 애니메이션 시작");
            for (int d = 1; d <= knockBackGA.Distance; d++)
            {
                Vector2Int pushedPos = knockBackGA.TargetPos + knockBackGA.Direction * d;
                yield return TokenSystem.Instance.MoveToken(target, pushedPos, true, false);
            }
            //Tween tween = DOTween.To(() =>
            //          knockBackGA.Caster.TokenTransform.positionXY,
            //          v => knockBackGA.Caster.TokenTransform.positionXY = v,
            //          targetPos,
            //          0.7f * pushedDis
            //       );

            //yield return tween.WaitForCompletion();
        }

        if (chrash && chrashedTarget != null)
        {
            Debug.Log("충돌 데미지 획득");
            DealDamageGA dealDamageGA = new(crachDamage, new() { chrashedTarget }, target as CombatantView);
            ActionSystem.Instance.AddReaction(dealDamageGA);
        }
    }
}
