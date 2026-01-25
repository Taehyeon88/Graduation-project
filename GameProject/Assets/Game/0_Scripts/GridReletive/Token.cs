using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Token : MonoBehaviour
{
    private TokenModel tokenModel;
    private TokenData data;
    public void SetUp(TokenData data, float rotationStep)
    {
        this.data = data;
        tokenModel = Instantiate(data.TokenModel, transform.position, Quaternion.identity);
        tokenModel.Rotate(rotationStep);
    }
}
