using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using IsoTools;
using UnityEngine;

public class Test : MonoBehaviour
{
    [SerializeField] private IsoObject IsoObject;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //Tween tween = Utility.GetBezierTween(IsoObject, new(1, 1, 1), new(3, 1, 1), 0.3f);
            //tween.Play();
        }
    }
}
