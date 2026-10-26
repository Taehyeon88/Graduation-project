public enum StatusEffectType
{ 
    //1번은 삭제된 ARMOR(방어)의 자리 — 에셋에 정수로 직렬화되므로 값을 고정한다
    POWER = 0,           //힘
    WEAK = 2,            //취약 : 공격자 디버프
    VULNERABLE = 3,      //약화 : 피격자 디버프
    DODGE = 4,           //회피 : 다음 1회 공격·피해 무효
    CRITICAL = 5,        //치명타 : 공격 시 공격자 공격력의 50% 추가 피해, 발동 시 1 소모
    STUN = 6             //기절 : 자동 전투 차례의 모든 행동 불가, 차례를 건너뛸 때 1 소모
}
