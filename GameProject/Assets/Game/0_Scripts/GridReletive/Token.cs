using System.Collections;
using System.Collections.Generic;
using IsoTools;
using UnityEngine;

public class Token : MonoBehaviour
{
    public TokenModel TokenModel {  get; protected set; }
    public TokenData TokenData { get; protected set; }
    public IsoObject TokenTransform { get; protected set; }
}
