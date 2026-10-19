//유닛당 발동 횟수 상한이 있는 효과 (카운트는 효과가 아니라 Trait이 유닛별로 센다)
public interface ILimitedTrigger
{
    public int MaxTriggers { get; }   //0 = 무제한
}
