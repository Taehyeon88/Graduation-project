public class PromotionOption   // 승급 2택1 선택지 1개 (새 특성 획득 또는 가진 특성 강화)
{
    public bool IsEnhance { get; }          //true = 특성 강화, false = 새 특성 획득
    public int SlotIndex { get; }           //적용할 특성 칸
    public TraitData Before { get; }        //칸의 기존 특성 (새 특성 획득이면 null)
    public TraitData After { get; }         //선택 후 칸의 특성 (새 특성 또는 강화형)

    public PromotionOption(bool isEnhance, int slotIndex, TraitData before, TraitData after)
    {
        IsEnhance = isEnhance;
        SlotIndex = slotIndex;
        Before = before;
        After = after;
    }
}
