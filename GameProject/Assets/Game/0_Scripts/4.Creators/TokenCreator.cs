using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TokenType
{
    None, Hero, Enemy, HeroCaptain, EnemyFlag
}

public class TokenCreator : Singleton<TokenCreator>
{
    [SerializeField] private HeroPreview heroPreviewPrefab;
    [SerializeField] private Token heroTokenPrefab;
    [SerializeField] private Token enemyTokenPrefab;
    [SerializeField] private Token heroCaptainPrefab;
    [SerializeField] private Token enemyFlagPrefab;
    [SerializeField] private Transform isoWorld;      //토큰들을 생성할 부모 오브젝트

    private Token tokenPrefab;

    public Token CreateToken(TokenData data, TokenType tokenType, Vector3 isoPosition)
    {
        switch(tokenType)
        {
            case TokenType.None: tokenPrefab = null; break;
            case TokenType.Hero: tokenPrefab = heroTokenPrefab; break;
            case TokenType.Enemy: tokenPrefab = enemyTokenPrefab; break;
            case TokenType.HeroCaptain: tokenPrefab = heroCaptainPrefab; break;
            case TokenType.EnemyFlag: tokenPrefab = enemyFlagPrefab; break;
            default: tokenPrefab = null; break;
        }
        if (tokenPrefab == null)
        {
            Debug.LogError("TokenCreator : 토큰 생성 불가");
            return null;
        }

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
            case TokenType.HeroCaptain:
                HeroCaptainView heroCaptainView = token as HeroCaptainView;
                heroCaptainView.SetUp(data as HeroCaptainData);
                break;
            case TokenType.EnemyFlag:
                EnemyFlagView enemyFlagView = token as EnemyFlagView;
                enemyFlagView.SetUp(data as EnemyFlagData);
                break;
        }

        token.TokenTransform.position = isoPosition;

        return token;
    }

    public HeroPreview CreateTokenPreview(TokenData data)
    {
        HeroPreview preview = Instantiate(heroPreviewPrefab, isoWorld);
        preview.SetUp(data);
        return preview;
    }
}
