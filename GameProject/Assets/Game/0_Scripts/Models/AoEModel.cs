using System.Collections;
using System.Collections.Generic;
using IsoTools;
using UnityEngine;

public class AoEModel : MonoBehaviour
{
    public Sprite Sprite
    {
        get
        {
            if (sprite == null)
                sprite = GetComponentInChildren<SpriteRenderer>().sprite;
            return sprite;
        }
    }
    public IsoObject AoETransform
    {
        get
        {
            if(aoETransform == null)
                aoETransform = GetComponent<IsoObject>();
            return aoETransform;
        }
    }

    private Sprite sprite;
    private IsoObject aoETransform;
}
