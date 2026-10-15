using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Token/Token")]
public class TokenData : ScriptableObject   // 기물의 최소 단위 데이터
{
    [field: SerializeField] public int Id { get; private set; }
    public string Name => name;   //SO 에셋 이름
    [field: SerializeField] public Sprite Sprite { get; private set; }
}
