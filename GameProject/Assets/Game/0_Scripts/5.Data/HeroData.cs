using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Token/Hero")]
public class HeroData : CombatantData
{
    [field: SerializeField] public int APCost { get; private set; }   //카드로 배치할 때 드는 AP
    [field: SerializeField] public int Critical { get; private set; }   //치명타 확률(%) - 자동 전투 종료 시 이 확률로 CRITICAL 상태효과 획득 (HeroSystem)
    [field: SerializeField] public List<TraitData> PromotionTraits { get; private set; }   //승급으로 얻을 수 있는 특성 (고유 특성이 있으면 1개, 없으면 2개)
}
