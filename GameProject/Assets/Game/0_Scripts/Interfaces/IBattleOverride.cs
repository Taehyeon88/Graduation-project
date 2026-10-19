//자동 전투 행동을 특성 효과가 대체하는 효과 (true를 반환하면 기본 이동·정면 공격을 건너뜀)
public interface IBattleOverride
{
    public bool TryBattle(CombatantView owner);
}
