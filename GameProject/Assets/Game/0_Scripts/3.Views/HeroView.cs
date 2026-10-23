using UnityEngine;
public class HeroView : CombatantView
{
    public int Id => TokenData.Id;

    protected override Vector2Int Forward => Vector2Int.right;
    protected override bool IsTarget(Token token) => token is EnemyView || token is ObstacleView;
    //적 배치 위치 직전 (배치 위치가 없는 라인은 마지막 열을 적 배치 열로 간주)
    protected override int GetMoveLimitX(int line)
        => (EnemySystem.Instance.TryGetSetupX(line, out int x) ? x : TokenSystem.Instance.gridWidth - 1) - 1;

    public void SetUp(HeroData heroData)
    {
        SetUpBase(heroData);
    }
}
