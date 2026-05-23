using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TokenType
{
    Hero, Enemy, Wall
}

public class TokenCreator : Singleton<TokenCreator>
{
    [SerializeField] private TokenPreview previewPrefab;
    [SerializeField] private Token heroTokenPrefab;
    [SerializeField] private Token enemyTokenPrefab;
    [SerializeField] private Token wallTokenPrefab;
    [SerializeField] private Transform isoWorld;      //토큰들을 생성할 부모 오브젝트

    private Token tokenPrefab;

    public Token CreateToken(TokenData data, TokenType tokenType, Vector3 isoPosition)
    {
        switch(tokenType)
        {
            case TokenType.Hero: tokenPrefab = heroTokenPrefab; break;
            case TokenType.Enemy: tokenPrefab = enemyTokenPrefab; break;
            case TokenType.Wall: tokenPrefab = wallTokenPrefab; break;
            default: tokenPrefab = null; break;
        }
        if (tokenPrefab == null) return null;

        Token token = Instantiate(tokenPrefab, isoWorld);

        switch (tokenType)
        {
            case TokenType.Hero: 
                HeroView heroView = token as HeroView;
                heroView.SetUp(data as HeroData);
                break;
            case TokenType.Enemy: 
                EnemyView enemyView = token as EnemyView;
                enemyView.SetUp(data as EnemyData);
                break;
            case TokenType.Wall:
                WallView wallView = token as WallView;
                wallView.SetUp(data as WallData);
                break;
                
        }

        token.TokenTransform.position = isoPosition;

        return token;
    }

    public TokenPreview CreateTokenPreview(TokenData data, Vector3 isoPosition)
    {
        TokenPreview preview = Instantiate(previewPrefab, isoWorld);
        preview.SetUp(data, isoPosition);
        return preview;
    }
}
