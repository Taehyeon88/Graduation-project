using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEditor.Tilemaps;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class TokenSystem : Singleton<TokenSystem> //몬스터 및 영웅 세팅 | 몬스터, 건물 추가 및 삭제 (게임 중) | 토큰 이동, 등
{
    public const float CellSize = 1f;
    [SerializeField] private TokenGrid grid;

    public HeroView HeroView { get; private set; }
    public List<EnemyView> EnemyViews { get; private set; } = new();

    private Dictionary<Token, Vector2Int> gridPosByToken = new();
    private TokenPreview preview;
    private TokenData tokenData;
    private List<Vector2Int> heroSetupPositions = new();
    private List<Vector2Int> movedPath = new();

    /// </summary>
    /// <param name="tokenDatas"></param>
    /// 전투 시작시, 모든 몬스터들 비어있는 그리드에 랜덤 배치 함수
    public void StartSettingEnemys(List<TokenData> tokenDatas, List<Vector2Int> canSetupPositions)
    {
        canSetupPositions = grid.GetCanSetPositions(canSetupPositions);
        foreach (TokenData tokenData in tokenDatas)
        {
            EnemyData enemyData = tokenData as EnemyData;

            Token token = TokenCreator.Instance.CreateToken(
                    enemyData,
                    TokenType.Enemy,
                    transform.position,
                    0f
                );
            Vector2Int gridPosition = grid.SetTokenRendomly(token, canSetupPositions);
            Vector3 position = grid.GridToWorldPosition(gridPosition);
            token.transform.position = position;

            if (token is EnemyView enemyToken)
            {
                EnemyViews.Add(enemyToken);
                gridPosByToken.Add(token, gridPosition);
            }
        }
    }

    /// <summary>
    /// 전투 중, 지정된 위치로 몬스터 배치 함수
    /// </summary>
    /// <param name="enemyData"></param>
    /// <param name="position"></param>
    public void AddEnemy(EnemyData enemyData, Vector3 position)
    {
        Vector3 snappedPos = GetSnappedCenterPosition(position);
        if (!grid.CanSet(snappedPos))
        {
            Debug.Log("해당 위치에는 적을 생성할 수 없습니다.");
            return;
        }
        PlaceToken(snappedPos, enemyData, TokenType.Enemy);
    }
    /// <summary>
    /// 특정 몬스터를 그리드에서 제거하는 함수
    /// </summary>
    /// <param name="enemyView"></param>
    /// <returns></returns>
    public IEnumerator RemoveEnemy(EnemyView enemyView)
    {
        EnemyViews.Remove(enemyView);
        grid.ResetToken(gridPosByToken[enemyView]);
        Tween tween = enemyView.transform.DOScale(Vector3.zero, 0.25f);
        yield return tween.WaitForCompletion();
        Destroy(enemyView.gameObject);
    }


    /// <summary>
    /// 전투 시작시, 영웅 배치를 위해서 실행되는 함수
    /// </summary>
    /// <param name="tokenData"></param>
    public void StartSetHero(TokenData tokenData, List<Vector2Int> heroSetupPositions)
    {
        this.tokenData = tokenData;
        this.heroSetupPositions = heroSetupPositions;
        foreach (var gridPos in heroSetupPositions)
        {
            Vector3 position = grid.GridToWorldPosition(gridPos);
            if (grid.CanSetByGridPos(gridPos))
                VisualGridCreator.Instance.CreateHeroVisualGrid(position, Color.green);
            else
                VisualGridCreator.Instance.CreateHeroVisualGrid(position, Color.red);
        }
        InteractionSystem.Instance.SetInteraction(InteractionCase.SetHero, UpdateHeroPreview);
    }

    /// <summary>
    /// 영웅 프리뷰를 업데이트할 함수
    /// </summary>
    /// <param name="tokenData"></param>
    private void UpdateHeroPreview(bool isSelect)
    {
        Vector3 mousePosition = MouseUtil.GetMousePositionInWorldSpace();
        if (preview != null)
            HandlePreView(mousePosition, isSelect);
        else
            preview = TokenCreator.Instance.CreateTokenPreview(tokenData, mousePosition);
    }

    /// <summary>
    /// 현재 영웅 프리뷰의 위치에 영웅을 설정 여부를 체크 및 생성하는 함수
    /// </summary>
    /// <param name="mouseWorldPosition"></param>
    /// <param name="isSelect"></param>
    private void HandlePreView(Vector3 mouseWorldPosition, bool isSelect)
    {
        preview.transform.position = mouseWorldPosition;
        Vector3 TokenPosition = preview.TokenModel.GetTokenPosition();
        bool canSet = grid.CanSet(TokenPosition, heroSetupPositions);
        if (canSet)
        {
            TokenPosition = preview.transform.position = GetSnappedCenterPosition(TokenPosition);
            preview.ChangeState(TokenPreview.TokenPreViewState.Positive);
            if (isSelect)
            {
                PlaceToken(TokenPosition, preview.Data, TokenType.Hero);
                VisualGridCreator.Instance.RemoveHeroVisualGrid();
                Destroy(preview.gameObject);
                tokenData = null;
                preview = null;
                heroSetupPositions = null;
            }
        }
        else preview.ChangeState(TokenPreview.TokenPreViewState.Negative);
    }

    /// <summary>
    /// 영웅 혹은 몬스터가 현재 이동가능 범위내의 모든 그리드를 전달하는 함수
    /// </summary>
    /// <param name="token"></param>
    /// <param name="maxDistance"></param>
    /// <returns></returns>
    public List<Vector3> GetCanMovePlace(Token token, int maxDistance)
    {
        Vector2Int start = gridPosByToken[token];
        var girds = FindPathBFS.FindAllPath(grid.simpleGrid, start, maxDistance);
        List<Vector3> list = new();
        foreach (var gird in girds)
        {
            Vector3 pos = grid.GridToWorldPosition(gird);
            list.Add(pos);
        }
        return list;
    }
    /// <summary>
    /// 영웅 혹은 몬스터가 현재 이동가능 범위내에서 목표지점까지의 최단 거리 전달 함수
    /// </summary>
    /// <param name="token"></param>
    /// <param name="endPosition"></param>
    /// <returns></returns>
    public List<Vector2Int> GetShortestPath(Token token, Vector3 endPosition)
    {
        Vector2Int start = gridPosByToken[token];
        Vector2Int end = grid.WorldToGirdPosition(endPosition);
        return FindPathBFS.FindPath(grid.simpleGrid, start, end);
    }

    /// <summary>
    ///현재 턴의 영웅 혹은 몬스터가 현재 턴에 이동한 모든 그리드들 받기
    /// </summary>
    /// <returns></returns>
    public List<Vector3> GetMovedPath()
    {
        List<Vector3> list = new();
        foreach (var pos in movedPath)
        {
            list.Add(grid.GridToWorldPosition(pos));
        }
        return list;
    }

    /// <summary>
    /// 토큰(영웅, 적, 건물) 이동 함수
    /// </summary>
    /// <param name="token"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public IEnumerator MoveToken(Token token, Vector2Int p)
    {
        grid.SetTokenByGridPos(token, p);
        movedPath.Add(gridPosByToken[token]);
        gridPosByToken[token] = p;
        Vector3 targetPost = grid.GridToWorldPosition(p);
        Tween tween = token.gameObject.transform.DOMove(targetPost, 1f);
        yield return tween.WaitForCompletion();
    }
    /// <summary>
    /// 영웅 혹은 몬스터 이동 턴 종료후, 필수!! 실행 초기화 함수
    /// </summary>
    public void ResetMovedPath()
    {
        for (int i = 0; i < movedPath.Count - 1; i++)
        {
            grid.ResetToken(movedPath[i]);
        }
        movedPath.Clear();
    }
    /// <summary>
    /// 좌표변환해서 받는 함수 : 그리드 -> 월드 or 월드 -> 그리드
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    public Vector3 GridToWorldPosition(Vector2Int p) => grid.GridToWorldPosition(p);
    public Vector2Int WorldToGridPosition(Vector3 p) => grid.WorldToGirdPosition(p);

    /// <summary>
    /// 특정 토큰의 현재 위치를 받아가는 함수
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public Vector2Int GetTokenGridPosition(Token token) => gridPosByToken[token];
    public Vector3 GetTokenWorldPosition(Token token) => grid.GridToWorldPosition(gridPosByToken[token]);

    public int GetDistacne(Token token, Vector3 endPos)
    {
        Vector2Int current = gridPosByToken[token];
        Vector2Int target = grid.WorldToGirdPosition(endPos);
        return Mathf.Max(Mathf.Abs(current.x - target.x), Mathf.Abs(current.y - target.y));
    }

    /// <summary>
    /// 특정 위치에 토큰(영웅, 적, 건물)을 생성하는 함수
    /// </summary>
    /// <param name="setPosition"></param>
    private void PlaceToken(Vector3 setPosition, TokenData data, TokenType tokenType)
    {
        Token token = TokenCreator.Instance.CreateToken(
                data,
                tokenType,
                preview.transform.position,
                0f
            );

        Vector2Int gridPos = grid.SetToken(token, setPosition);
        if (gridPos != null)
            gridPosByToken.TryAdd(token, gridPos);

        if (token is HeroView heroView)
            HeroView = heroView;
        else if (token is EnemyView enemyView)
            EnemyViews.Add(enemyView);
    }

    /// <summary>
    /// Global 좌표계 위치에서 Grid 위치에 맞게 snap 해주는 함수
    /// </summary>
    /// <param name="tokenPosition"></param>
    /// <returns></returns>
    private Vector3 GetSnappedCenterPosition(Vector3 tokenPosition)
    {
        int sx = Mathf.FloorToInt(tokenPosition.x);
        int sz = Mathf.FloorToInt(tokenPosition.z);

        float centerX = sx + CellSize / 2f;
        float centerz = sz + CellSize / 2f;
        return new Vector3(centerX, 0, centerz);
    }
}
