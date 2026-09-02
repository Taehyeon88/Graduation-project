using System.Collections;
using System.Collections.Generic;
using IsoTools;
using UnityEngine;

public class Token : MonoBehaviour
{
    [field: SerializeField] public SpriteRenderer SpriteRenderer { get; protected set; }
    public TokenData TokenData { get; protected set; }
    public IsoObject TokenTransform { get; protected set; }

    protected void SetUpBaseBase(TokenData tokenData, IsoObject isoObject)
    {
        TokenData = tokenData;
        SpriteRenderer.sprite = tokenData.Sprite;    //이미지 셋업
        TokenTransform = isoObject;                  //isomertric용 transform 셋업

        //높낮이 조절
        float y = SpriteRenderer.gameObject.transform.position.y;
        Vector3 pos = SpriteRenderer.gameObject.transform.position;
        SpriteRenderer.gameObject.transform.position = new Vector3(pos.x, y + tokenData.HRange, pos.z);
    }
}
