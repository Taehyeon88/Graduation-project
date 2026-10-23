//발판 위에 올라선/떠난 용병에게 적용되는 기능의 베이스 (TerrainView가 발판별 사본을 보유하고, TerrainSystem이 이동·소환 시 호출)
[System.Serializable]
public abstract class TerrainEffect
{
    public abstract void OnEnter(CombatantView unit);   //발판 위에 올라섰을 때

    public abstract void OnExit(CombatantView unit);    //발판을 떠났을 때

    //TerrainView가 발판별 사본을 갖기 위한 얕은 복사 (참조형 상태를 가진 기능은 오버라이드해서 새로 만든다)
    public virtual TerrainEffect Clone() => (TerrainEffect)MemberwiseClone();
}
