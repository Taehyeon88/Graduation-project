using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TokenModel : MonoBehaviour   //캐릭터 프리팹
{
    public Sprite Sprite
    {
        get
        {
            if (sprite == null)
                sprite = GetComponentInChildren<SpriteRenderer>().sprite;
            return sprite;
        }
        set
        {
            sprite = value;
            GetComponentInChildren<SpriteRenderer>().sprite = value;
        }
    }
    private Sprite sprite;
}
