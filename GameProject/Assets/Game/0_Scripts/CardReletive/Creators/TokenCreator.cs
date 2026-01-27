using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TokenType
{
    Hero, Enemy, Builing
}

public class TokenCreator : Singleton<TokenCreator>
{
    [SerializeField] private TokenPreview previewPrefab;
    [SerializeField] private Token heroTokenPrefab;
    [SerializeField] private Token enemyTokenPrefab;
    [SerializeField] private Transform tokenView;      //토큰들을 생성할 부모 오브젝트

    private Token tokenPrefab;

    public Token CreateToken(TokenData data, TokenType tokenType, Vector3 position, float rotationStep)
    {
        switch(tokenType)
        {
            case TokenType.Hero: tokenPrefab = heroTokenPrefab; break;
            case TokenType.Enemy: tokenPrefab = enemyTokenPrefab; break;
            default: tokenPrefab = null; break;
        }
        if (tokenPrefab == null) return null;

        Token token = Instantiate(tokenPrefab, position, Quaternion.identity, tokenView);

        switch (tokenType)
        {
            case TokenType.Hero: 
                HeroView heroView = token as HeroView;
                heroView.SetUp(data as HeroData, rotationStep);
                break;
            case TokenType.Enemy: 
                EnemyView enemyView = token as EnemyView;
                enemyView.SetUp(data as EnemyData, rotationStep);
                break;
        }
        return token;
    }

    public TokenPreview CreateTokenPreview(TokenData data, Vector3 position)
    {
        TokenPreview preview = Instantiate(previewPrefab, position, Quaternion.identity);
        preview.SetUp(data);
        return preview;
    }
}
