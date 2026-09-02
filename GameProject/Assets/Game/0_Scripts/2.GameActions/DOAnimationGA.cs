using DG.Tweening;
using UnityEngine;

public class DOAnimationGA : GameAction
{
    public Sequence Sequence {  get; private set; }
    public Tween Tween { get; private set; }

    public DOAnimationGA(Sequence sequence)
    {
        this.Sequence = sequence;
    }
    public DOAnimationGA(Tween tween)
    {
        this.Tween = tween;
    }
}
