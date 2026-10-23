using SerializeReferenceEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Terrain")]
public class TerrainData : ScriptableObject   // 발판(지형지물) 데이터
{
    [field: SerializeField] public int Id { get; private set; }
    public string Name => name;   //SO 에셋 이름
    [field: SerializeField, TextArea] public string Description { get; private set; }   //발판 설명
    [field: SerializeField] public Sprite Sprite { get; private set; }
    [field: SerializeReference, SR] public TerrainEffect Effect { get; private set; }   //올라선 용병에게 적용되는 기능
}
