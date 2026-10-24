using UnityEngine;

[CreateAssetMenu(menuName = "Data/Token/Hero")]
public class HeroData : CombatantData
{
    [field: SerializeField] public int APCost { get; private set; }   //카드로 배치할 때 드는 AP
    [field: SerializeField] public int Critical { get; private set; }   //치명타 확률(%) - 자동 전투 종료 시 이 확률로 CRITICAL 상태효과 획득 (HeroSystem)
}
