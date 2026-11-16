[System.Serializable]
public abstract class StarCondition   //★3 스테이지 특수 조건 (StageData가 [SerializeReference]로 조립, 승리한 판에서만 판정)
{
    public abstract string Description { get; }        //정보 팝업에 표시할 조건 문구
    public abstract bool IsMet(StageResult result);
}
