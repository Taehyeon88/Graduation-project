using UnityEngine;

//★3 템플릿: N라운드 안에 승리
public class RoundLimitStarCondition : StarCondition
{
    [SerializeField] private int maxRounds = 5;

    public override string Description => $"{maxRounds}라운드 안에 승리";

    public override bool IsMet(StageResult result) => result.Rounds <= maxRounds;
}
