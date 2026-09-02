using SerializeReferenceEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Perk")]
public class PerkData : ScriptableObject
{
    [field : SerializeField] public int Id {  get; private set; }
    [field : SerializeField] public Sprite Image { get; private set; }
    [field : SerializeField] public string Description { get; private set; }
    [field : SerializeReference, SR] public Perk Perk { get; private set; }
}
