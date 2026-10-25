using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TokenType
{
    None, Hero, Enemy, HeroCaptain, EnemyFlag, Obstacle
}

public class TokenCreator : Singleton<TokenCreator>
{
    [SerializeField] private HeroPreview heroPreviewPrefab;
    [SerializeField] private Token heroTokenPrefab;
    [SerializeField] private Token enemyTokenPrefab;
    [SerializeField] private Token heroCaptainPrefab;
    [SerializeField] private Token enemyFlagPrefab;
    [SerializeField] private Token obstaclePrefab;
    [SerializeField] private TerrainView terrainPrefab;
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
            case TokenType.Obstacle: tokenPrefab = obstaclePrefab; break;
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
                heroView.SetUp(new Hero(data as HeroData));   //보상 후보 표시용 (임시 개체)
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
            case TokenType.Obstacle:
                ObstacleView obstacleView = token as ObstacleView;
                obstacleView.SetUp(data as ObstacleData);
                break;
        }

        token.TokenTransform.position = isoPosition;

        return token;
    }

    //덱의 영웅 개체로 영웅 토큰 생성 (보너스·획득 특성 반영)
    public HeroView CreateHero(Hero hero, Vector3 isoPosition)
    {
        HeroView heroView = Instantiate(heroTokenPrefab, isoWorld) as HeroView;
        heroView.SetUp(hero);
        heroView.TokenTransform.position = isoPosition;
        return heroView;
    }

    //발판은 토큰이 아니므로 그리드에 등록하지 않는다
    public TerrainView CreateTerrain(TerrainData data, Vector2Int gridPosition)
    {
        if (terrainPrefab == null)
        {
            Debug.LogError("TokenCreator : 발판 생성 불가");
            return null;
        }

        TerrainView terrain = Instantiate(terrainPrefab, isoWorld);
        terrain.SetUp(data, gridPosition);
        return terrain;
    }

    public HeroPreview CreateTokenPreview(TokenData data)
    {
        HeroPreview preview = Instantiate(heroPreviewPrefab, isoWorld);
        preview.SetUp(data);
        return preview;
    }
}
