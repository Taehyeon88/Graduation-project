using SerializeReferenceEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Trait")]
public class TraitData : ScriptableObject
{
    [field : SerializeField] public int Id {  get; private set; }
    [field : SerializeField] public Sprite Image { get; private set; }
    [field : SerializeField] public string Description { get; private set; }
    [field : SerializeReference, SR] public TraitCondition Condition { get; private set; }
    [field : SerializeReference, SR] public TraitEffect Effect { get; private set; }
}
