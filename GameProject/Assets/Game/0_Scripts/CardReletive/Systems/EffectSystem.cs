using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EffectSystem : MonoBehaviour
{
    void OnEnable()
    {
        ActionSystem.AttachPerformer<PerformEffectGA>(PerformEffectPerformer);
    }
    void OnDisable()
    {
        ActionSystem.DetachPerformer<PerformEffectGA>();
    }
    private IEnumerator PerformEffectPerformer(PerformEffectGA performEffectGA)
    {
        if (!performEffectGA.IsSkiping)
        {
            string cardTypeString = performEffectGA.EffectInfo.cardType.ToString();
            cardTypeString = cardTypeString.Substring(0, cardTypeString.IndexOf("_"));

            Debug.Log($"이름: {cardTypeString}");

            if (cardTypeString == "Attack")  //전투 공격 연출 사용
            {
                var type = (performEffectGA.EffectInfo.cardType, performEffectGA.EffectInfo.cardSubType);

                //시작 모션
                var casterPos = TokenSystem.Instance.GetTokenPosition(HeroSystem.Instance.HeroView);
                PlayVisualEffectGA playVisualEffectGA = new(type, 0, HeroSystem.Instance.HeroView, casterPos, performEffectGA.EffectInfo.targetPoses[0] - casterPos);
                ActionSystem.Instance.AddReaction(playVisualEffectGA);

                //피격 이펙트 전달
                DamageSystem.Instance.DamageVFX = VisualEffectSystem.Instance.GetHitVEInfo(type).Item2;

                //실행할 행동
                GameAction effectAction = performEffectGA.Effect.GetGameAction(performEffectGA.EffectInfo);
                ActionSystem.Instance.AddReaction(effectAction);

                //회수 모션
                PlayVisualEffectGA playVisualEffectGA2 = new(type, 1, HeroSystem.Instance.HeroView, casterPos, Vector2Int.zero);
                ActionSystem.Instance.AddReaction(playVisualEffectGA2);
            }
            else
            {
                GameAction effectAction = performEffectGA.Effect.GetGameAction(performEffectGA.EffectInfo);
                ActionSystem.Instance.AddReaction(effectAction);
            }

            yield return null;
        }
    }
}
