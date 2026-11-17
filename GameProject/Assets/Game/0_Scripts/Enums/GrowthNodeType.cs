//공용 트리 노드 효과 종류 — 각 적용 지점이 GameSystem.GetGrowthBonus로 조회 (노드 에셋이 정수로 저장하므로 값 고정, 확장만)
public enum GrowthNodeType
{
    NONE = 0,                 //효과 준비 중 (구매 불가)
    CAPTAIN_HEALTH = 1,       //용병대장 체력
    RETALIATE_DAMAGE = 2,     //응수 피해 (용병대장 공격력)
    FREE_DRAW = 3,            //전투당 무료 카드 뽑기 횟수
    PLACEMENT_DISCOUNT = 4,   //전투당 카드 배치 AP −1 횟수 (최소 1)
    START_DRAW = 5,           //시작 드로우 장수
    MAX_AP = 6,               //최대 AP
    HERO_CRITICAL = 7,        //영웅 치명타 확률 (%)
    PLACED_HERO_HEALTH = 8,   //카드로 배치되는 영웅 체력
    PLACED_HERO_SPEED = 9,    //카드로 배치될 때 속도 +1, 전투당 횟수
    CRITICAL_DAMAGE = 10,     //치명타 추가 피해 비율 (%p)
    HERO_DAMAGE = 11,         //모든 영웅 공격력
    DAGGER_DAMAGE = 12,       //날카로운 단도 피해
    DAGGER_USES = 13,         //날카로운 단도 전투당 횟수
    FLAG_DAMAGE = 14,         //영웅이 적 깃발에 주는 피해 +1, 전투당 횟수
    DECK_SLOT = 15            //덱 선택 슬롯
}
