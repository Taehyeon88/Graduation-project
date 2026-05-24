using System.Collections;
using System.Collections.Generic;
using IsoTools;
using UnityEngine;

public class Token : MonoBehaviour
{
    [SerializeField] protected Transform wrapper;
    [SerializeField] protected Transform genTransform;

    public TokenModel TokenModel {  get; protected set; }
    public TokenData TokenData { get; protected set; }
    public IsoObject TokenTransform { get; protected set; }

    protected void SetUpBaseBase(TokenData tokenData, IsoObject isoObject)
    {
        foreach (Transform child in wrapper)    //몬스터 & 플레이어 모델 셋업
            Destroy(child.gameObject);
        this.TokenData = tokenData;
        TokenModel = Instantiate(tokenData.TokenModel, genTransform ? genTransform.position : wrapper.position, Quaternion.identity, wrapper);

        TokenTransform = isoObject;             //몬스터 & 플레이어 isomertric용 transform 셋업

        if(tokenData.Sprite != null)
            TokenModel.Sprite = tokenData.Sprite;
    }
}
