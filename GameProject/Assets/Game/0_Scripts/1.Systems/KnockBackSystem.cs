using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class KnockBackSystem : MonoBehaviour
{
    private const int crachDamage = 1;    //충돌 데미지

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<KnockBackGA>(KnockBackGAPerformer);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<KnockBackGA>();
    }

    //Performer

    /// <summary>
    /// 특정 위치의 대상을 특정 방향으로 거리만 넉백시키는 함수
    /// </summary>
    /// <param name="knockBackGA"></param>
    /// <returns></returns>
    private IEnumerator KnockBackGAPerformer(KnockBackGA knockBackGA)
    {
        CombatantView caster = knockBackGA.Caster;      //밀친 사람
        Vector2Int targetPos = knockBackGA.TargetPos;   //넉백 시킬 대상의 위치
        int distance = knockBackGA.Distance;            //넉백 시킬 거리 (대상이 밀쳐지는 거리)
        Vector2Int direction = knockBackGA.Direction;   //넉백 시킬 방향

        int pushedDis = 0;               //실제 넉백 밀리는 거리
        bool chrash = false;             //충돌 여부

        Debug.Log("넉백 성공!!");

        var target = TokenSystem.Instance.GetTokenByPosition(targetPos) as CombatantView;  //밀쳐질 대상

        //넉백 효과 판단 처리
        for (int d = 1; d <= distance; d++)
        {
            //밀려날 위치 받기
            Vector2Int pushedPos = targetPos + knockBackGA.Direction * d;

            if (!TokenSystem.Instance.IsGridEmpty(pushedPos)) chrash = true;

            if (!chrash) pushedDis = d;
            else break;
        }

        //넉백 애니메이션
        MoveGA moveGA = null;
        if (pushedDis > 0)
        {
            Debug.Log("밀리는 애니메이션 시작");
            Vector2Int pushedPos = targetPos + direction * pushedDis;
            moveGA = new(target, pushedPos, true);
            ActionSystem.Instance.AddReaction(moveGA);
        }

        if (chrash)
        {
            Debug.Log("충돌 데미지 획득");
            DealDamageGA dealDamageGA = new(crachDamage, new() { target }, knockBackGA.Caster);
            ActionSystem.Instance.AddReaction(dealDamageGA);
        }
        yield return null;
    }
}
