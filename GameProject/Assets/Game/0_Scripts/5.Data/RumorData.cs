using SerializeReferenceEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Rumor")]
public class RumorData : ScriptableObject
{
    [field : SerializeField] public int Id { get; private set; }
    [field : SerializeField] public Sprite Image { get; private set; }
    [field : SerializeField] public string Description { get; private set; }
    [field : SerializeReference, SR] public Rumor Rumor { get; private set; }

    //에셋 파일명에서 '_' 뒤쪽을 잘라 만든 이름 (Trait.Title과 같은 규칙)
    public string Title
    {
        get
        {
            int index = name.IndexOf("_");
            return index >= 0 ? name.Substring(index + 1) : name;
        }
    }
}
