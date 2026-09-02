using System.Collections.Generic;
using SerializeReferenceEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Skill")]
public class SkillData : ScriptableObject
{
    [field: SerializeField] public int Id { get; private set; }
    [field: SerializeField] public string Description { get; private set; }
    [field: SerializeField] public int Limit { get; private set; }
    [field: SerializeField] public Sprite Image { get; private set; }
    [field: SerializeField] public SkillType SkillType { get; private set; }
    [field: SerializeField] public SkillAbility SkillAbility { get; private set; }
}
