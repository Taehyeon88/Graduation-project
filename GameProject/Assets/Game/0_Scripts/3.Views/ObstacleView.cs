public class ObstacleView : CombatantView   // 파괴 가능 장애물 (Forward가 zero라 스스로 행동하지 않음)
{
    public void SetUp(ObstacleData obstacleData)
    {
        SetUpBase(obstacleData);
    }
}
