using DG.Tweening;
using System.Collections;
using UnityEngine;

public class AnimationSystem : Singleton<AnimationSystem>
{
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<DOAnimationGA>(DOAnimationPerformer);
    }
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<DOAnimationGA>();
    }

    private IEnumerator DOAnimationPerformer(DOAnimationGA animationGA)
    {
        if (animationGA.Tween != null)
        {
            Debug.Log("리턴 애니메이션");
            animationGA.Tween.Restart();
            yield return animationGA.Tween.WaitForCompletion();
        }
        else if (animationGA.Sequence != null)
        {
            animationGA.Sequence.Restart();
            yield return animationGA.Sequence.WaitForCompletion();
        }
    }
}
