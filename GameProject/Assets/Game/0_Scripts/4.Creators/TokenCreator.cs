using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TokenType
{
    None, Hero, Enemy, WaveCore
}

public class TokenCreator : Singleton<TokenCreator>
{
    [SerializeField] private HeroPreview heroPreviewPrefab;
    [SerializeField] private Token heroTokenPrefab;
    [SerializeField] private Token enemyTokenPrefab;
    [SerializeField] private Token waveCorePrefab;
    [SerializeField] private Transform isoWorld;      //토큰들을 생성할 부모 오브젝트

    private Token tokenPrefab;

    public Token CreateToken(TokenData data, TokenType tokenType, Vector3 isoPosition)
    {
        switch(tokenType)
        {
            case TokenType.None: tokenPrefab = null; break;
            case TokenType.Hero: tokenPrefab = heroTokenPrefab; break;
            case TokenType.Enemy: tokenPrefab = enemyTokenPrefab; break;
            case TokenType.WaveCore: tokenPrefab = waveCorePrefab; break;
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
            case TokenType.WaveCore:
                WaveCoreView coreView = token as WaveCoreView;
                coreView.SetUp(data as WaveCoreData);
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
