using System.Collections;
using System.Collections.Generic;
using IsoTools;
using UnityEngine;

public class Token : MonoBehaviour
{
    [field: SerializeField] public SpriteRenderer Model { get; protected set; }
    public TokenData TokenData { get; protected set; }
    public IsoObject TokenTransform { get; protected set; }
    public Transform Transform { get; protected set; }
    public Vector3 Defualt_Direction { get; protected set; }    //Model과 Iso 오브젝트의 거리 차이
    public Vector3 Defualt_Position { get; protected set; }     //Model의 원래 위치
    protected void SetUpBaseBase(TokenData tokenData, IsoObject isoObject)
    {
        TokenData = tokenData;
        Model.sprite = tokenData.Sprite;    //이미지 셋업
        TokenTransform = isoObject;         //isomertric용 transform 셋업
        Transform = transform.GetChild(0);  //Parent 오브젝트 transform 값

        //높낮이 조절
        float y = Model.gameObject.transform.position.y;
        Vector3 pos = Model.gameObject.transform.position;
        Model.gameObject.transform.position = new Vector3(pos.x, y + tokenData.HRange, pos.z);

        Defualt_Position = Model.transform.localPosition;
        Defualt_Direction = Model.transform.position - gameObject.transform.position;
    }
}
