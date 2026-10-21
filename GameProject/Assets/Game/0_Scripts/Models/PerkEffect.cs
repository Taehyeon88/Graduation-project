[System.Serializable]
public abstract class PerkEffect
{
    public abstract void PerformReaction(GameAction action);

    //Perk이 사본을 갖기 위한 얕은 복사 (참조형 상태를 가진 효과는 오버라이드해서 새로 만든다)
    public virtual PerkEffect Clone() => (PerkEffect)MemberwiseClone();
}
