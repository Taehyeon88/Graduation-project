using UnityEngine;

//2 유연한: 중립 타일에 배치할 수 있고, 배치 시 소모되는 AP가 줄어든다 (훅은 쓰지 않음, 인터페이스로만 동작)
public class FlexibleRumor : Rumor, IAPCostModifier, INeutralPlacement
{
    [SerializeField] private int apCostReduction = 1;

    public int APCostBonus => -apCostReduction;
    public bool CanPlaceOnNeutral => true;
}
