using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class TokenSystem : Singleton<TokenSystem> //몬스터 및 영웅 세팅 | 몬스터, 건물 추가 및 삭제 (게임 중) | 토큰 이동, 등
{
    public const float CellSize = 1f;
    [SerializeField] private TokenPreview previewPrefab;
    [SerializeField] private TokenGrid grid;

    public HeroView HeroView { get; private set; }
    public List<EnemyView> EnemyViews { get; private set; } = new();
    public bool startSetting { get; private set; } = false;

    private Dictionary<Token, Vector2Int> gridPosByToken = new();
    private TokenPreview preview;
    private TokenData tokenData;
    public void StartSetting(TokenData tokenData)
    {
        this.tokenData = tokenData;
        startSetting = true;
    }

    public void StartSettingEnemys(List<TokenData> tokenDatas)  //턴 종료시, HeroToke 혹은 EnemyTokens, 등 초기화 기능 미구현 상태
    {
        Debug.Log($"적 생성 시작");
        foreach (TokenData tokenData in tokenDatas)
        {
            EnemyData enemyData = tokenData as EnemyData;

            Token token = TokenCreator.Instance.CreateToken(
                    enemyData,
                    TokenType.Enemy,
                    transform.position,
                    0f
                );
            Vector2Int gridPosition = grid.SetTokenRendomly(token);
            Vector3 position = grid.GridToWorldPosition(gridPosition);
            token.transform.position = position;

            if (token is EnemyView enemyToken)
            {
                Debug.Log($"적 토큰 이름: {enemyToken.name}");
                EnemyViews.Add(enemyToken);
                gridPosByToken.Add(token, gridPosition);
            }
        }
    }

    public void AddEnemy(EnemyData enemyData, Vector3 position)
    {
        Vector3 snappedPos = GetSnappedCenterPosition(position);
        if (!grid.CanSet(snappedPos))
        {
            Debug.Log("해당 위치에는 적을 생성할 수 없습니다.");
            return;
        }
        Token token = TokenCreator.Instance.CreateToken(enemyData, TokenType.Enemy, snappedPos, 180f);
        Vector2Int gridPos = grid.SetToken(token, snappedPos);
        if (token is EnemyView enemyToken)
        {
            EnemyViews.Add(enemyToken);
            gridPosByToken.Add(token, gridPos);
        }
    }

    public IEnumerator RemoveEnemy(EnemyView enemyView)
    {
        EnemyViews.Remove(enemyView);
        grid.ResetToken(gridPosByToken[enemyView]);
        Tween tween = enemyView.transform.DOScale(Vector3.zero, 0.25f);
        yield return tween.WaitForCompletion();
        Destroy(enemyView.gameObject);
    }

    private void Update()
    {
        if (startSetting)
        {
            Vector3 mousePosition = MouseUtil.GetMousePositionInWorldSpace();
            if (preview != null)
            {
                HandlePreView(mousePosition);
            }
            else
            {
                preview = TokenCreator.Instance.CreateTokenPreview(tokenData, mousePosition);
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                preview.Rotate(90f);
            }
        }
        else   //토큰 이동
        {
            //Vector3 mousePosition = GetMouseWorldPosition();
            //if (Input.GetKeyDown(KeyCode.Space))
            //{
            //    Vector2Int start = gridPosByToken[HeroView];
            //    Vector2Int end = grid.WorldToGirdPosition(mousePosition);
            //    FindPathBFS.SetGrid(grid.grid);
            //    List<Vector2Int> path = FindPathBFS.FindPath(start, end);

            //    if (path != null)
            //    {
            //        StartCoroutine(StartMove(HeroView, path));
            //    }
            //}
        }
    }
    private void HandlePreView(Vector3 mouseWorldPosition)   //플레이어 배치 (현재)
    {
        preview.transform.position = mouseWorldPosition;
        Vector3 TokenPosition = preview.TokenModel.GetTokenPosition();
        bool canSet = grid.CanSet(TokenPosition);
        if (canSet)
        {
            preview.transform.position = GetSnappedCenterPosition(TokenPosition);
            preview.ChangeState(TokenPreview.TokenPreViewState.Positive);
            if (Input.GetMouseButtonDown(0))
            {
                PlaceToken(TokenPosition);
            }
        }
    }
    private void PlaceToken(Vector3 setPosition)    //플레이어용 (현재)
    {
        Token token = TokenCreator.Instance.CreateToken(
                preview.Data, 
                TokenType.Hero, 
                preview.transform.position, 
                preview.TokenModel.Rotation
            );
        Destroy(preview.gameObject);

        Vector2Int girdPos = grid.SetToken(token, setPosition);
        if (girdPos != null)
        {
            gridPosByToken.TryAdd(token, girdPos);
        }

        if (token is HeroView heroView)
            this.HeroView = heroView;
        else if (token is EnemyView enemyView)
            EnemyViews.Add(enemyView);

        ResetValue();
    }
    private void HandleMoveToken(Token token)
    {

    }
    private IEnumerator StartMove(Token token, List<Vector2Int> path)
    {
        foreach (var p in path)
        {
            yield return MoveToken(token, p);
        }
    }
    private IEnumerator MoveToken(Token token, Vector2Int gridPosition)
    {
        gridPosByToken[token] = gridPosition;
        Vector3 targetPost = grid.GridToWorldPosition(gridPosition);
        Tween tween = token.gameObject.transform.DOMove(targetPost, 1f);
        yield return tween.WaitForCompletion();
    }


    private Vector3 GetSnappedCenterPosition(Vector3 tokenPosition)
    {
        int sx = Mathf.FloorToInt(tokenPosition.x);
        int sz = Mathf.FloorToInt(tokenPosition.z);

        float centerX = sx + CellSize / 2f;
        float centerz = sz + CellSize / 2f;
        return new Vector3(centerX, 0, centerz);
    }

    private void ResetValue()
    {
        tokenData = null;
        startSetting = false;
        preview = null;

    }
}
