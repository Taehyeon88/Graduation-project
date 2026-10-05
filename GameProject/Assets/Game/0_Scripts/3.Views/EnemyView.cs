using UnityEngine;

public class EnemyView : CombatantView
{
    public int Id => TokenData.Id;
    public string EnemyName => TokenData.Name;            //적 이름
    public Sprite EnemySprite => TokenData.Sprite;        //적 이미지

    public void SetUp(EnemyData enemyData)
    {
        SetUpBase(enemyData.Health, enemyData.Health, enemyData);
    }
}
