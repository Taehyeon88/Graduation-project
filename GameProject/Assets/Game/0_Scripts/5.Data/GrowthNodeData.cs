using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/GrowthNode")]
public class GrowthNodeData : ScriptableObject
{
    [field : SerializeField] public string Code { get; private set; }               //표시용 코드 (C1, R3 …)
    [field : SerializeField] public GrowthBranchType Branch { get; private set; }
    [field : SerializeField] public int MaxLevel { get; private set; } = 1;        //1~10, 1레벨 = 별 1
    [field : SerializeField] public GrowthNodeType Type { get; private set; }       //NONE이면 효과 준비 중
    [field : SerializeField] public int AmountPerLevel { get; private set; } = 1;  //레벨당 효과량 (레벨만큼 누적)
    [field : SerializeField] public string EffectFormat { get; private set; }       //효과 설명, {0}에 레벨 × AmountPerLevel (예: "용병대장 체력 +{0}")
    [field : SerializeField] public List<GrowthNodePrerequisite> Prerequisites { get; private set; } = new();   //모두 충족해야 열림 (비면 본부에서 바로 열림)

    //에셋 파일명에서 '_' 뒤쪽을 잘라 만든 이름 (RumorData.Title과 같은 규칙)
    public string Title
    {
        get
        {
            int index = name.IndexOf("_");
            return index >= 0 ? name.Substring(index + 1) : name;
        }
    }
}
