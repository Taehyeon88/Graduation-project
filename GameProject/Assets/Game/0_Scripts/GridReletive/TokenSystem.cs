using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

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
    private bool tokenIsMoving = false;

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
    /// 영웅 이동 시작 처리하는 함수
    /// </summary>
    public void StartHeroMove()
    {
        InteractionSystem.Instance.SetInteraction(InteractionCase.HeroMove, UpdateHeroMove);
    }

    /// <summary>
    /// 영웅 토큰 이동 업데이트 함수
    /// </summary>
    /// <param name="isSelect"></param>
    private void UpdateHeroMove(bool isSelect)
    {
        if (tokenIsMoving) return;

        Vector3 mousePosition = MouseUtil.GetMousePositionInWorldSpace();
        if (isSelect)
        {
            Vector2Int start = gridPosByToken[HeroView];
            Vector2Int end = grid.WorldToGirdPosition(mousePosition);
            FindPathBFS.SetGrid(grid.grid);
            List<Vector2Int> path = FindPathBFS.FindPath(start, end);

            if (path != null)
            {
                StartCoroutine(MoveToken(HeroView, path));
            }
        }
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
    /// 토큰(영웅, 적, 건물) 이동 함수
    /// </summary>
    /// <param name="token"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    private IEnumerator MoveToken(Token token, List<Vector2Int> path)
    {
        tokenIsMoving = true;
        foreach (var p in path)
        {
            grid.SetTokenByGridPos(token, p);
            grid.ResetToken(gridPosByToken[token]);
            gridPosByToken[token] = p;
            Vector3 targetPost = grid.GridToWorldPosition(p);
            Tween tween = token.gameObject.transform.DOMove(targetPost, 1f);
            yield return tween.WaitForCompletion();
        }
        tokenIsMoving = false;
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
