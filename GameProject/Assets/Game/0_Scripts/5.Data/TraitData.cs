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
    [field : SerializeField] public TraitData Enhanced { get; private set; }   //영웅 승급으로 강화했을 때 바뀌는 강화형(특성+) — 비어 있으면 강화 불가

    [Header("작업용 변수s")]
    [field : SerializeField] public string ConditionName;
    [field : SerializeField] public ReactionTiming timing;
    [field : SerializeField] public string EffectName;
}
