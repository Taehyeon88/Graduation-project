using UnityEngine;

public class EnemyView : CombatantView
{
    public int Id => TokenData.Id;

    protected override int MinBaseDamage => 1;
    protected override Vector2Int Forward => Vector2Int.left;
    protected override bool IsTarget(Token token) => token is HeroView || token is ObstacleView;
    //영웅 배치 위치 직전 (배치 위치가 없는 라인은 첫 열을 영웅 배치 열로 간주)
    protected override int GetMoveLimitX(int line)
        => (HeroSystem.Instance.TryGetSetupX(line, out int x) ? x : 0) + 1;
    public string EnemyName => TokenData.Name;            //적 이름
    public Sprite EnemySprite => TokenData.Sprite;        //적 이미지

    public void SetUp(EnemyData enemyData)
    {
        SetUpBase(enemyData);
    }
}
