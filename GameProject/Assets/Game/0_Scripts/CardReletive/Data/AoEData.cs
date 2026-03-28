using System.Collections;
using System.Collections.Generic;
using SerializeReferenceEditor;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/AoE")]
public class AoEData : ScriptableObject
{
    [field: SerializeField] public AoEModel AoEModel { get; private set; }            //장판 모델(스프라이트 포함)
    [field: SerializeField] public AoEFieldType AoEFieldType { get; private set; }    //장판 계열(오브젝트 or 필드)
    [field: SerializeField] public AoEType AoEType { get; private set;}               //장판 타입
    [field: SerializeField] public int EntryDamage { get; private set; }              //진입 피해
    [field: SerializeField] public int TurnDamage { get; private set; }               //턴당 피해

    public bool UseCountBased = false;
    [ShowIf("UseCountBased")]
    public int DurationCount;

    [field: Tooltip("UseCountBased가 ture일 때, 사용하지 않습니다!")]
    [field: SerializeField] public int DurationTurn { get; private set; }             //지속 턴 수

    [field: Tooltip("AddStatusEffect만 받기 위한 변수입니다!")]
    [field: SerializeReference, SR] public List<Effect> statusEffects { get; private set; }    //본인에게만 부여하는 상태효과
}
