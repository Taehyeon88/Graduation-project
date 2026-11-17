//공용 트리 노드 표시 상태 (GrowthNodeSystem.GetState → GrowthNodeUI 색·안내 분기)
public enum GrowthNodeState
{
    NOT_IMPLEMENTED,    //효과 준비 중 (구매 불가)
    LOCKED,             //선행 조건 미충족
    MAX,                //최대 레벨
    NOT_ENOUGH_STARS,   //열렸지만 별 부족
    PURCHASABLE         //구매 가능
}
