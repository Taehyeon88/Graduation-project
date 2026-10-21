using SerializeReferenceEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Perk")]
public class PerkData : ScriptableObject
{
    [field : SerializeField] public int Id { get; private set; }
    [field : SerializeField] public Sprite Image { get; private set; }
    [field : SerializeField] public string Description { get; private set; }
    [field : SerializeField] public int MaxTriggersPerBattle { get; private set; }   //전투당 발동 상한 (0이면 무제한)
    [field : SerializeReference, SR] public PerkCondition Condition { get; private set; }
    [field : SerializeReference, SR] public PerkEffect Effect { get; private set; }
}
