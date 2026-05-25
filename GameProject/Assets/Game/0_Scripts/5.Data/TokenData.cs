using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Token/Token")]
public class TokenData : ScriptableObject   // 프리팹을 갖고 있는데 데이터
{
    [field: SerializeField] public TokenModel TokenModel { get; private set; }
    [field: SerializeField] public string Name { get; private set; }
    [field: SerializeField] public Sprite Sprite { get; private set; }

    public bool useHealth;
    [ShowIf("useHealth")]
    public int Health;
}
