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
        GameAction effectAction = performEffectGA.Effect.GetGameAction(performEffectGA.TargetPoses, performEffectGA.MyView);
        ActionSystem.Instance.AddReaction(effectAction);

        yield return null;
    }
}
