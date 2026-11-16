using UnityEngine;

//★3 템플릿: 특정 몬스터 N마리 이상 처치
public class KillEnemyStarCondition : StarCondition
{
    [SerializeField] private EnemyData targetEnemy;
    [SerializeField] private int killCount = 1;

    public override string Description => $"{(targetEnemy != null ? targetEnemy.Name : "?")} {killCount}마리 이상 처치";

    public override bool IsMet(StageResult result)
        => targetEnemy != null && result.KillCounts.TryGetValue(targetEnemy, out int kills) && kills >= killCount;
}
