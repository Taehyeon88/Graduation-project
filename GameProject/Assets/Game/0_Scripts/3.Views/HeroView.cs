using UnityEngine;
public class HeroView : CombatantView
{
    public int Id => TokenData.Id;

    protected override Vector2Int Forward => Vector2Int.right;
    protected override bool IsTarget(Token token) => token is EnemyView || token is WaveCoreView;

    public void SetUp(HeroData heroData)
    {
        SetUpBase(heroData);
    }
}
