using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using IsoTools;
using UnityEngine;

public class TokenSystem : Singleton<TokenSystem> //몬스터 및 영웅 세팅 | 몬스터, 건물 추가 및 삭제 (게임 중) | 토큰 이동, 등
{
    public const float CellSize = 1f;
    [SerializeField] private TokenGrid grid;
    [field : SerializeField] public IsoWorld IsoWorld { get; private set; }

    public HeroView HeroView { get; private set; }
    public List<EnemyView> EnemyViews { get; private set; } = new();
    public List<WallView> WallViews { get; private set; } = new();

    private Dictionary<Token, Vector2Int> gridPosByToken = new();
    private TokenPreview preview;
    private TokenData tokenData;
    private List<Vector2Int> heroSetupPositions = new();
    private List<Vector2Int> movedPath = new();

    /// <summary>
    /// 정해진 위치좌표들에 각 벽들을 생성하는 함수
    /// </summary>
    /// <param name="wallDatas"></param>
    /// <param name="setupPositions"></param>
    public void StartSetWalls(List<TokenData> wallDatas, List<Vector2Int> setupPositions)
    {
        int index = 0;
        foreach (var wallData in wallDatas)
        {
            if (setupPositions.Count > index)
            {
                Vector2Int pos = setupPositions[index];

                Token token = TokenCreator.Instance.CreateToken(
                        wallData,
                        TokenType.Wall,
                        transform.position
                    );
                token.TokenTransform.position = new(pos.x, pos.y, 1);

                grid.SetToken(token, pos);
                WallViews.Add(token as WallView);
                gridPosByToken.Add(token, pos);
            }

            index++;
        }
    }
    /// <summary>
    /// 전투 시작시, 모든 몬스터들 비어있는 그리드에 랜덤 배치 함수
    /// </summary>
    /// <param name="tokenDatas"></param>
    /// <param name="canSetupPositions"></param>
    public void StartSettingEnemys(List<TokenData> tokenDatas, List<Vector2Int> canSetupPositions)
    {
        EnemyViews.Clear();

        canSetupPositions = grid.GetCanSetPositions(canSetupPositions);
        foreach (TokenData tokenData in tokenDatas)
        {
            EnemyData enemyData = tokenData as EnemyData;

            Token token = TokenCreator.Instance.CreateToken(
                    enemyData,
                    TokenType.Enemy,
                    transform.position
                );
            Vector2Int gridPosition = grid.SetTokenRendomly(token, canSetupPositions);
            token.TokenTransform.position = new(gridPosition.x, gridPosition.y, 1);

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
        //Vector3 snappedPos = GetSnappedCenterPosition(position);
        //if (!grid.CanSet(snappedPos))
        //{
        //    Debug.Log("해당 위치에는 적을 생성할 수 없습니다.");
        //    return;
        //}
        //PlaceToken(snappedPos, enemyData, TokenType.Enemy);
        //EnemySystem.Instance?.EnemyAddEvent?.Invoke()
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
        gridPosByToken.Remove(enemyView);
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
            if (grid.CanSetByGridPos(gridPos))
                VisualGridCreator.Instance.CreateVisualGrid(gameObject.GetInstanceID(), gridPos, "Hero_SetUp_True");
            else
                VisualGridCreator.Instance.CreateVisualGrid(gameObject.GetInstanceID(), gridPos, "Hero_SetUp_False");
        }
        InteractionSystem.Instance.SetInteraction(InteractionCase.SetUp, UpdateHeroPreview);
    }

    /// <summary>
    /// 영웅 프리뷰를 업데이트할 함수
    /// </summary>
    /// <param name="tokenData"></param>
    private void UpdateHeroPreview(bool isSelect)
    {
        Vector3 isoPosition = IsoWorld.MouseIsoTilePosition(1f);

        if (preview != null)
            HandlePreView(isoPosition, isSelect);
        else
            preview = TokenCreator.Instance.CreateTokenPreview(tokenData, isoPosition);
    }

    /// <summary>
    /// 현재 영웅 프리뷰의 위치에 영웅을 설정 여부를 체크 및 생성하는 함수
    /// </summary>
    /// <param name="isoPosition"></param>
    /// <param name="isSelect"></param>
    private void HandlePreView(Vector3 isoPosition, bool isSelect)
    {
        preview.TokenTransform.position = isoPosition;

        Vector2Int gridPosition = new Vector2Int((int)isoPosition.x, (int)isoPosition.y);
        bool canSet = grid.CanSetByGridPos(gridPosition) && heroSetupPositions.Contains(gridPosition);
        if (canSet)
        {
            preview.ChangeState(TokenPreview.TokenPreViewState.Positive);
            if (isSelect)
            {
                PlaceToken(gridPosition, preview.TokenData, TokenType.Hero);
                VisualGridCreator.Instance.RemoveVisualGridById(gameObject.GetInstanceID());
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
    public List<Vector2Int> GetCanMovePlace(Token token, int maxDistance)
    {
        Debug.Log($"현재 위치: {gridPosByToken[token]}");
        Vector2Int start = gridPosByToken[token];
        var result = FindPathBFS.FindAllPath((int[,])grid.simpleGrid.Clone(), start, maxDistance);

        if (token is HeroView)
        {
            foreach (var r in result.ToList())
            {
                if (movedPath.Contains(r))
                    result.Remove(r);
            }
        }
        return result;
    }

    public List<Vector2Int> GetCanMovePlace2(Token token, int maxDistance)
    {
        List<Vector2Int> places = new();
        Vector2Int start = gridPosByToken[token];
        for (int dis = 1; dis <= maxDistance; dis++)
        {
            foreach (var dir in FindPathBFS.Dirs)
            {
                Vector2Int pos = start + dir * dis;
                if (!grid.CanSetByGridPos(pos)) continue;
                if (token is HeroView && movedPath.Contains(pos)) continue;

                places.Add(pos);
            }
        }
        return places;
    }

    /// <summary>
    /// 영웅 혹은 몬스터가 현재 이동가능 범위내에서 목표지점까지의 최단 거리 전달 함수
    /// </summary>
    /// <param name="token"></param>
    /// <param name="endPosition"></param>
    /// <returns></returns>
    public List<Vector2Int> GetShortestPath(Token token, Vector2Int isoPosition)
    {
        Vector2Int start = gridPosByToken[token];
        return FindPathBFS.FindPath((int[,])grid.simpleGrid.Clone(), start, isoPosition);
    }

    /// <summary>
    ///현재 턴의 영웅 혹은 몬스터가 현재 턴에 이동한 모든 그리드들 받기
    /// </summary>
    /// <returns></returns>
    public List<Vector2Int> GetMovedPath() => new(movedPath);

    /// <summary>
    /// 토큰(영웅, 적, 건물) 이동 함수
    /// </summary>
    /// <param name="token"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public IEnumerator MoveToken(Token token, Vector2Int targetPos, bool useAnimation = true, bool useMovedPath = true)
    {
        if (token is HeroView && useMovedPath)   //이전 위치를 이동한 경로에 저장 (영웅 한정)
        {
            if(!movedPath.Contains(gridPosByToken[token]))
            {
                movedPath.Add(gridPosByToken[token]);
                VisualGridCreator.Instance.ChangeVisualGrid(gridPosByToken[token], 
                    MoveSystem.Instance.gameObject.GetInstanceID(), 
                    "Hero_Move", 
                    "Hero_Moved"
                    );
            }
            if (!movedPath.Contains(targetPos))
            {
                movedPath.Add(targetPos);
                VisualGridCreator.Instance.ChangeVisualGrid(targetPos,
                    MoveSystem.Instance.gameObject.GetInstanceID(), 
                    "Hero_Move", 
                    "Hero_Moved"
                    );
            }
        }
        
        grid.ResetToken(gridPosByToken[token]);
        grid.SetToken(token, targetPos);
        gridPosByToken[token] = targetPos;

        if (useAnimation)
        {
            Tween tween = Utility.GetTween(token, targetPos, 1f);
            yield return tween.WaitForCompletion();
        }
    }
    /// <summary>
    /// 영웅만 이동 턴 종료후, 필수!! 실행 초기화 함수
    /// </summary>
    public void ResetMovedPath() => movedPath.Clear();
    /// <summary>
    /// 영웅만 이동한 경로에 포함 되는 그리드 존재 여부 확인 함수
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public bool CheckContainMovedPath(Vector2Int pos) => movedPath.Contains(pos);

    /// <summary>
    /// 특정 토큰의 현재 위치를 받아가는 함수
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public Vector2Int GetTokenPosition(Token token) => gridPosByToken[token];

    /// <summary>
    /// 현재 게임 위에 있는 모든 토큰 받아가는 함수
    /// </summary>
    /// <returns></returns>
    public List<Token> GetAllTokens() => gridPosByToken.Keys.ToList();

    /// <summary>
    /// 특정 위치의 토큰을 받아가는 함수
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public Token GetTokenByPosition(Vector2Int position)
    {
        foreach (var dic in gridPosByToken)
        {
            if (dic.Value == position)
                return dic.Key;
        }
        return null;
    }

    /// <summary>
    /// 해당 그리드 위치가 타일 범위 안인지 확인
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public bool IsBound(Vector2Int pos) => grid.IsBound(pos.x, pos.y);

    /// <summary>
    /// 해당 Token의 위치에서 목표 지점까지의 거리 계산 함수
    /// </summary>
    /// <param name="token"></param>
    /// <param name="endPos"></param>
    /// <returns></returns>
    public int GetDistance(Token token, Vector2Int endPos)
    {
        Vector2Int current = gridPosByToken[token];
        return Mathf.Max(Mathf.Abs(current.x - endPos.x), Mathf.Abs(current.y - endPos.y));
    }

    /// <summary>
    /// 특정 토큰에서 토큰으로의 거리를 구하는 함수
    /// </summary>
    /// <param name="to"></param>
    /// <param name="from"></param>
    /// <returns></returns>
    public Vector2Int GetDirection(Token to, Token from)
    {
        return gridPosByToken[to] - gridPosByToken[from];
    }

    /// <summary>
    /// 해당 토큰을 기준으로한 방향의 그리드 위치를 얻는 함수
    /// </summary>
    /// <param name="token"></param>
    /// <param name="direction"></param>
    /// <returns></returns>
    public Vector2Int GetDirectionPos(Token token, Vector2Int direction)
    {
        Vector2Int current = gridPosByToken[token];
        return current + direction;
    }

    /// <summary>
    /// 현재 토큰 주변으로 8방향의 그리드 좌표를 받아가는 함수
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public Vector2Int[] GetAroundGrids(Token token)
    {
        var dirs = FindPathBFS.Dirs;
        var result = new List<Vector2Int>();

        Vector2Int cur = gridPosByToken[token];
        for (int i = 0; i < dirs.Length; i++)
        {
            int nx = cur.x + dirs[i].x;
            int ny = cur.y + dirs[i].y;

            if (!grid.IsBound(nx, ny)) continue;

            result.Add(new Vector2Int(nx, ny));
        }
        return result.ToArray();
    }

    public List<CombatantView> TargetPosesToCombatants(List<Vector2Int> targetPoses)
    {
        //대상 기반
        List<CombatantView> combatants = new();
        foreach (var targetPos in targetPoses)
        {
            Token token = TokenSystem.Instance.GetTokenByPosition(targetPos);
            if (token != null)
                combatants.Add(token as CombatantView);
        }
        if (combatants.Count > 0) return combatants;
        return null;
    }

    /// <summary>
    /// 해당 그리드 좌표 사용 가능 여부 확인 (범위 안 & 토큰이 없는가 등)
    /// </summary>
    /// <param name="isPosition"></param>
    /// <returns></returns>
    public bool IsGridEmpty(Vector2Int isPosition, bool enemyException = false, bool heroException = false)
    {
        if (enemyException || heroException) 
            return grid.CanSetByGridPosExceptionToken(isPosition, enemyException, heroException);
        else return grid.CanSetByGridPos(isPosition);
    }

    /// <summary>
    /// 해당 위치의 필드/오브젝트를 새로운 타일(영역)으로 바꾸는 함수
    /// </summary>
    /// <param name="aoE"></param>
    /// <param name="pos"></param>
    public void SetAoE(IsoObject aoE, Vector2Int pos, AoEFieldType aoEFieldType)
    {
        if (aoEFieldType == AoEFieldType.Field) grid.SetField(aoE, pos);
        else if (aoEFieldType == AoEFieldType.Object) grid.SetObject(aoE, pos);
    }
    
    /// <summary>
    /// 해당 위치의 필드를 기본 필드로 되돌리는 함수 | 해당 위치의 오브젝트를 삭제하는 함수
    /// </summary>
    /// <param name="pos"></param>
    public void ResetAoE(Vector2Int pos, AoEFieldType aoEFieldType)
    {
        if (aoEFieldType == AoEFieldType.Field) grid.ResetField(pos);
        else if (aoEFieldType == AoEFieldType.Object) grid.ResetObject(pos);
    }

    //privates

    /// <summary>
    /// 특정 위치에 토큰(영웅, 적, 건물)을 생성하는 함수
    /// </summary>
    /// <param name="setPosition"></param>
    private void PlaceToken(Vector2Int setPosition, TokenData data, TokenType tokenType)
    {
        Token token = TokenCreator.Instance.CreateToken(
                data,
                tokenType,
                preview.TokenTransform.position
            );

        grid.SetToken(token, setPosition);

        Vector2Int gridPos = new((int)setPosition.x, (int)setPosition.y);
        if (gridPos != null)
            gridPosByToken.TryAdd(token, gridPos);

        if (token is HeroView heroView)
            HeroView = heroView;
        else if (token is EnemyView enemyView)
            EnemyViews.Add(enemyView);
    }
}
