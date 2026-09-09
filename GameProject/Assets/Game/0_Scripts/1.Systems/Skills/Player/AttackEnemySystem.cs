using DG.Tweening;
using IsoTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackEnemySystem : MonoBehaviour
{
    [Header("Direct Elements")]
    [SerializeField] private IsoObject arrowTrans;

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
        Sequence seq = DOTween.Sequence();
        Vector2Int current_Pos = TokenSystem.Instance.API.GetTokenPosition(attackEnemyGA.MyView);
        Token myToken = attackEnemyGA.MyView;
        bool endTween = false;  //피격으로 넘어가기

        var animationType = attackEnemyGA.animationType;
        if (animationType == HeroAnimationType.CLOSE_ATTACK)
        {
            Vector2Int target_Pos = attackEnemyGA.TargetPoses[0];
            Vector2 direction = Utility.GetSignVector2Int(target_Pos - current_Pos);

            Tween ready = Utility.GetModelTween(myToken, current_Pos, direction, 0.7f, 0.15f, Ease.InQuad)
                       .OnStart(() => SoundSystem.Instance.PlaySound(1003));

            Tween back = Utility.GetModelBackTween(myToken, 0.05f, Ease.Linear);

            seq.Append(ready)
               .AppendCallback(() =>
               {
                   endTween = true;
                   SoundSystem.Instance.PlaySound(1002);
               })
               .Insert(ready.Duration() + 0.08f, back);
        }
        else if (animationType == HeroAnimationType.RANGED_ATTACK)
        {
            Vector2Int target_Pos = attackEnemyGA.TargetPoses[0];
            Vector2Int direction = Utility.GetSignVector2Int(target_Pos - current_Pos);

            Tween ready = Utility.GetModelTween(myToken, current_Pos, -direction, 0.4f, 0.15f, Ease.InQuad)
                                 .OnStart(() => SoundSystem.Instance.PlaySound(1003));

            Tween arrowTween = Utility.GetArrowBezierTween(
                    arrowTrans,
                    arrowTrans.transform.GetChild(0).transform,
                    Utility.Vector2IntToVector3(current_Pos, 1),
                    Utility.Vector2IntToVector3(attackEnemyGA.TargetPoses[0], 1),
                    0.35f,
                    Ease.Linear).OnStart(() => SoundSystem.Instance.PlaySound(1001));

            Tween back = Utility.GetModelBackTween(myToken, 0.12f, Ease.Linear);

            seq.Append(ready)
               .AppendCallback(() => arrowTrans.gameObject.SetActive(true))
               .Insert(ready.Duration() - 0.03f, arrowTween)
               .Insert(ready.Duration() + 0.25f, back)
               .InsertCallback(ready.Duration() + arrowTween.Duration() - 0.03f, () =>
               {
                   endTween = true;
               })
               .InsertCallback(ready.Duration() + arrowTween.Duration() + 0.07f,() =>
               {
                   arrowTrans.gameObject.SetActive(false);
               });
        }

        yield return new WaitUntil(() => endTween);

        //피격 로직 실행
        var targets = Utility.PositionsToCombantViews(attackEnemyGA.TargetPoses);
        if (targets.Count > 0)
        {
            DealDamageGA dealDamageGA = new(attackEnemyGA.Amount, targets, attackEnemyGA.MyView);
            ActionSystem.Instance.AddReaction(dealDamageGA);
        }
    }
}
