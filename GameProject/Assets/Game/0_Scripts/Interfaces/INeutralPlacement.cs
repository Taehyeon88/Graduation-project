//기본 배치 열의 중립지대에 배치할 수 있는 소문 (Card.CanPlaceOnNeutral이 확인)
public interface INeutralPlacement
{
    public bool CanPlaceOnNeutral { get; }
}
