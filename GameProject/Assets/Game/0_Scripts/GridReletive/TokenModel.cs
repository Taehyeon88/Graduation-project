using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TokenModel : MonoBehaviour   //캐릭터 프리팹
{
    public float Rotation => transform.eulerAngles.y;
    private TokenShapeUnit shapeUnit;
    private void Awake()
    {
        shapeUnit = GetComponentInChildren<TokenShapeUnit>();
    }
    public void Rotate(float rotationStep)
    {
        transform.Rotate(new Vector3(0, rotationStep, 0));
    }
    public Vector3 GetTokenPosition()
    {
        return shapeUnit.transform.position;
    }
}
