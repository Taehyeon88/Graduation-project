using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

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
        if (knockBackGA.IsSingle)
        {
            int pushedDis = 0;               //실제 넉백 밀리는 거리
            bool chrash = false;

            Debug.Log("넉백 성공!!");

            ////넉백될 대상 찾기
            var target = TokenSystem.Instance.GetTokenByPosition(knockBackGA.TargetPos) as CombatantView;

            //넉백 효과 판단 처리
            for (int d = 1; d <= knockBackGA.Distance; d++)
            {
                //밀려날 위치 받기
                Vector2Int pushedPos = knockBackGA.TargetPos + knockBackGA.Direction * d;

                if (!TokenSystem.Instance.IsBound(pushedPos)) chrash = true;
                else if (TokenSystem.Instance.GetTokenByPosition(pushedPos) != null) chrash = true;

                Debug.Log($"정보 - 밀린 위치 : {pushedPos}, 충돌 여부: {chrash}");

                if (!chrash) pushedDis = d;
                else break;
            }

            //넉백 애니메이션
            MoveGA moveGA = null;
            if (pushedDis > 0)
            {
                Debug.Log("밀리는 애니메이션 시작");
                Vector2Int pushedPos = knockBackGA.TargetPos + knockBackGA.Direction * pushedDis;
                moveGA = new(target, pushedPos, true);
                ActionSystem.Instance.AddReaction(moveGA);
            }
            if (chrash)
            {
                Debug.Log("충돌 데미지 획득");
                DealDamageGA dealDamageGA = new(crachDamage, new() { target as CombatantView }, knockBackGA.Caster);
                ActionSystem.Instance.AddReaction(dealDamageGA);
            }
        }
        else
        {
            //for (int i = 0; i < knockBackGA.TargetPoses.Count; i++)
            //{
            //    Vector2Int targetPos = knockBackGA.TargetPoses[i];
            //    Vector2Int direction = knockBackGA.Directions[i];
            //    bool chrash = false;
            //    int pushedDis = 0;               //실제 넉백 밀리는 거리

            //    Debug.Log("넉백 성공!!");

            //    ////넉백될 대상 찾기
            //    Token target = TokenSystem.Instance.GetTokenByPosition(targetPos);

            //    //넉백 효과 판단 처리
            //    for (int d = 1; d <= knockBackGA.Distance; d++)
            //    {
            //        //밀려날 위치 받기
            //        Vector2Int pushedPos = targetPos + direction * d;

            //        if (!TokenSystem.Instance.IsBound(pushedPos)) chrash = true;
            //        else if (TokenSystem.Instance.GetTokenByPosition(pushedPos) != null) chrash = true;

            //        Debug.Log($"정보 - 밀린 위치 : {pushedPos}, 충돌 여부: {chrash}");

            //        if (!chrash) pushedDis = d;
            //        else break;
            //    }

            //    //넉백 애니메이션
            //    if (pushedDis > 0)
            //    {
            //        Debug.Log("밀리는 애니메이션 시작");
            //        for (int d = 1; d <= knockBackGA.Distance; d++)
            //        {
            //            Vector2Int pushedPos = targetPos + direction * d;
            //            if (i == knockBackGA.TargetPoses.Count - 1)
            //            {
            //                yield return TokenSystem.Instance.MoveToken(target, pushedPos, true, false);
            //            }
            //            else TokenSystem.Instance.MoveToken(target, pushedPos, true, false);
            //        }
            //    }

            //    if (chrash)
            //    {
            //        Debug.Log("충돌 데미지 획득");
            //        DealDamageGA dealDamageGA = new(crachDamage, chrashedTargets, knockBackGA.Caster);
            //        ActionSystem.Instance.AddReaction(dealDamageGA);
            //    }
            //}
        }
        yield return null;
    }
}
