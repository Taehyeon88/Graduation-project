//플레이어가 직접 사용하는 스킬의 베이스 (스킬별 대상 규칙과 실행할 GA만 정의, 횟수·AP·턴 제한은 PlayerSkillSystem이 공통 처리)
[System.Serializable]
public abstract class PlayerSkill
{
    public abstract bool IsValidTarget(Token target);

    public abstract GameAction CreateAction(Token target);
}
