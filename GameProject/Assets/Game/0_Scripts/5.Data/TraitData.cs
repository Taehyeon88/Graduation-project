using SerializeReferenceEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Trait")]
public class TraitData : ScriptableObject
{
    [field : SerializeField] public int Id {  get; private set; }
    [field : SerializeField] public TraitOwnerType OwnerType { get; private set; }   //특성을 가지는 대상 구분 (영웅·몬스터·기타)
    [field : SerializeField] public Sprite Image { get; private set; }
    [field : SerializeField] public string Description { get; private set; }
    [field : SerializeReference, SR] public TraitCondition Condition { get; private set; }
    [field : SerializeReference, SR] public TraitEffect Effect { get; private set; }
    
    [Header("작업용 변수s")]
    [field : SerializeField] public string ConditionName;
    [field : SerializeField] public ReactionTiming timing;
    [field : SerializeField] public string EffectName;
}
