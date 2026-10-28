using UnityEngine;
public class HeroView : CombatantView
{
    public int Id => TokenData.Id;
    public Hero Hero { get; private set; }   //덱의 영웅 개체 (보너스·획득 특성 보유)

    protected override int MinBaseDamage => 1;
    protected override Vector2Int Forward => Vector2Int.right;
    protected override bool IsTarget(Token token) => token is EnemyView || token is ObstacleView;
    //적 배치 위치 직전 (배치 위치가 없는 라인은 마지막 열을 적 배치 열로 간주)
    protected override int GetMoveLimitX(int line)
        => (EnemySystem.Instance.TryGetSetupX(line, out int x) ? x : TokenSystem.Instance.gridWidth - 1) - 1;

    private Rumor rumor;   //유닛별 소문 사본

    protected override void OnDisable()
    {
        base.OnDisable();
        rumor?.OnRemove();
    }

    public void SetUp(Hero hero)
    {
        Hero = hero;
        SetUpBase(hero.Data, hero.MaxHealth, hero.Damage, hero.Speed, hero.Traits);

        rumor = hero.Rumor?.Rumor?.Clone();
        rumor?.OnAdd(this);
    }
}
