using UnityEngine;

[CreateAssetMenu(menuName = "Data/Structure")]
public class StructureData : ScriptableObject
{
    [field: SerializeField] public int ID { get; private set; }
    [field: SerializeField] public Sprite Sprite { get; private set; }
    [field: SerializeField, TextArea] public string Description { get; private set; }   //구조물 설명
    [field: SerializeField] public string SceneName { get; private set; }               //선택 시 이동할 씬 이름 (예: 일반 전투 → GameDemoScene)
    [field: SerializeField] public StageData StageData { get; private set; }             //전투 구조물의 스테이지 데이터 (비전투 구조물은 null)
}
