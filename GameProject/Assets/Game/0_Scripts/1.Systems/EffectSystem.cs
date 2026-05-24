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
        if(performEffectGA.IsSkiping) yield break;

        HeroVisualEffectSystem.Instance.PlayVisualEffectPreGameAction(
            performEffectGA.EffectInfo.cardType, 
            performEffectGA.EffectInfo.cardSubType, 
            performEffectGA.EffectInfo.targetPoses
            );

        GameAction effectAction = performEffectGA.Effect.GetGameAction(performEffectGA.EffectInfo); 
        ActionSystem.Instance.AddReaction(effectAction);

        HeroVisualEffectSystem.Instance.PlayVisualEffectPostGameAction(
            performEffectGA.EffectInfo.cardType, 
            performEffectGA.EffectInfo.cardSubType, 
            performEffectGA.EffectInfo.targetPoses
            );
    }
}
