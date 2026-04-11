using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardAbilityType 
{
    AddNextAdjDamage,       //다음 인접 카드 50% 증가
    LockDiscarding,         //버려지는 것을 막는다.
    GetCardByDiscardPile,   //버려진 카드 더미에서 카드 한장 가져옴
    AddNextAdjBleeding,     //다음 인접 카드 사용시, 출혈 2 부여
}
