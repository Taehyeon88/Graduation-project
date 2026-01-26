using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TokenCreator : Singleton<TokenCreator>
{
    [SerializeField] private EnemyView enemyViewPrefab;

    public EnemyView CreateEnemyView(EnemyData enemyData, Vector3 position, Quaternion rotation)
    {
        EnemyView enemyView = Instantiate(enemyViewPrefab, position, rotation);
        enemyView.SetUp(enemyData);
        return enemyView;
    }

    public Token CreateToken(TokenData data, Token tokenPrefab, Vector3 position, float rotationStep)
    {
        Token token = Instantiate(tokenPrefab, position, Quaternion.identity, transform);
        token.SetUp(data, rotationStep);
        return token;
    }
}
