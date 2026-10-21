using SerializeReferenceEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/PlayerSkill")]
public class PlayerSkillData : ScriptableObject
{
    [field: SerializeField] public int Id { get; private set; }
    [field: SerializeField] public string Description { get; private set; }
    [field: SerializeField] public Sprite Image { get; private set; }
    [field: SerializeField] public int MaxUses { get; private set; }   //전투당 사용 횟수 제한
    [field: SerializeReference, SR] public PlayerSkill Skill { get; private set; }
}
