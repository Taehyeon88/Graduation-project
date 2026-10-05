using UnityEngine;

public class EnemyView : CombatantView
{
    public int Id => TokenData.Id;

    protected override Vector2Int Forward => Vector2Int.left;
    protected override bool IsTarget(Token token) => token is HeroView;
    public string EnemyName => TokenData.Name;            //적 이름
    public Sprite EnemySprite => TokenData.Sprite;        //적 이미지

    public void SetUp(EnemyData enemyData)
    {
        SetUpBase(enemyData);
    }
}
