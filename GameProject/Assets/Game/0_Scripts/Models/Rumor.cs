[System.Serializable]
public abstract class Rumor
{
    public virtual void OnAdd(HeroView owner) { }   //배치 직후 호출 (스탯 적용·구독 시작)
    public virtual void OnRemove() { }              //소유 뷰가 사라질 때 호출 (구독 해제)

    //HeroView가 유닛별 사본을 갖기 위한 얕은 복사 (참조형 상태를 가진 소문은 오버라이드해서 새로 만든다)
    public virtual Rumor Clone() => (Rumor)MemberwiseClone();
}
