using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EffectProcessor : MonoBehaviour
{
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<PerformEffectGA>(PerformEffectGAPerformer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<PerformEffectGA>();
    }
    private IEnumerator PerformEffectGAPerformer(PerformEffectGA performEffectGA)
    {
        GameAction effectAction = performEffectGA.Effect.GetGameAction(performEffectGA.TargetPoses, performEffectGA.Caster);
        ActionSystem.Instance.AddReaction(effectAction);

        yield return null;
    }
}
