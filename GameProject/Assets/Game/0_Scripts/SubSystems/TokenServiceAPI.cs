using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TokenServiceAPI : MonoBehaviour
{
    [Header("TokenGrid")]
    [field: SerializeField] private TokenGrid grid;

    private IReadOnlyDictionary<Token, Vector2Int> gridPosByToken => grid.gridPosByToken;
    private IReadOnlyDictionary<Vector2Int, Token> tokenBygridPos => grid.tokenByGridPos;

    /// <summary>
    /// 영웅 혹은 몬스터가 현재 이동가능 범위내의 모든 그리드를 전달하는 함수
    /// </summary>
    /// <param name="token"></param>
    /// <param name="maxDistance"></param>
    /// <returns></returns>
    public List<Vector2Int> GetCanMovePlace(Token token, int maxDistance)
    {
        //Debug.Log($"현재 위치: {gridPosByToken[token]}");
        Vector2Int start = gridPosByToken[token];
        var result = UtilityBFS.FindALLRoots(start, maxDistance);
        return result;
    }

    public List<Vector2Int> GetAllAroundPlaces(Vector2Int currentPosition, int maxDistance, bool exceptEnemy = false, bool exceptHero = false, bool exceptDestructable = false)
    {
        return UtilityBFS.FindAllPlaces(currentPosition, maxDistance, exceptEnemy, exceptHero, exceptDestructable);
    }

    /// <summary>
    /// 영웅 혹은 몬스터가 현재 이동가능 범위내에서 목표지점까지의 최단 거리 전달 함수
    /// </summary>
    /// <param name="token"></param>
    /// <param name="endPosition"></param>
    /// <returns></returns>
    public List<Vector2Int> GetShortestPath(Token token, Vector2Int goal)
    {
        Vector2Int start = gridPosByToken[token];
        return UtilityBFS.FindShortestPath(grid.GetSimpleGridCopied(), start, goal);
    }

    /// <summary>
    /// 특정 토큰의 현재 위치를 받아가는 함수
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public Vector2Int GetTokenPosition(Token token)
    {
        if(token != null && gridPosByToken.ContainsKey(token))
            return gridPosByToken[token];

        return Vector2Int.down;
    }

    /// <summary>
    /// 현재 게임 위에 있는 모든 토큰 받아가는 함수
    /// </summary>
    /// <returns></returns>
    public List<Token> GetAllTokens() => gridPosByToken.Keys.ToList();

    /// <summary>
    /// 해당 토큰이 존재 여부 확인 함수
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public bool IsTokenExist(Token token) => gridPosByToken.ContainsKey(token);

    /// <summary>
    /// 특정 위치의 토큰을 받아가는 함수
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public Token GetTokenByPosition(Vector2Int position)
    {
        if(tokenBygridPos.ContainsKey(position))
            return tokenBygridPos[position];

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
        return Mathf.Abs(current.x - endPos.x) + Mathf.Abs(current.y - endPos.y);
    }
    public int GetDistance(Token token, Token token2)
    {
        Vector2Int current = gridPosByToken[token];
        Vector2Int endPos = gridPosByToken[token2];
        return Mathf.Abs(current.x - endPos.x) + Mathf.Abs(current.y - endPos.y);
    }
    public int GetDistance(Vector2Int startPos, Vector2Int endPos)
    {
        return Mathf.Abs(startPos.x - endPos.x) + Mathf.Abs(startPos.y - endPos.y);
    }

    /// <summary>
    /// 해당 토큰과 위치의 최단 거리 반환 함수
    /// </summary>
    /// <param name="token"></param>
    /// <param name="endPos"></param>
    /// <returns></returns>
    public int GetMinDistance(Token token, Vector2Int endPos)
    {
        var path = GetShortestPath(token, endPos);
        if (path == null) return 0;

        return path.Count;
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
    public Vector2Int GetPositionByDirection(Token token, Vector2Int direction)
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
        var dirs = UtilityBFS.Dirs;
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
            Token token = GetTokenByPosition(targetPos);
            if (token != null)
                combatants.Add(token as CombatantView);
        }
        if (combatants.Count > 0) return combatants;
        return null;
    }

    /// <summary>
    /// 특정 토큰을 기준으로 방향, 거리 값만큼 떨어진 그리드 위치 찾는 함수
    /// </summary>
    /// <param name="token"></param>
    /// <param name="direction"></param>
    /// <param name="distance"></param>
    /// <returns></returns>
    public Vector2Int GetTargetPosByDirection(Token token, Vector2Int direction, int distance = 1)
    {
        Vector2Int pos = GetTokenPosition(token);
        Vector2 dir = Utility.GetSignVector2(direction);
        return pos + new Vector2Int((int)dir.x, (int)dir.y) * distance;
    }

    /// <summary>
    /// 해당 그리드 좌표 사용 가능 여부 확인 (범위 안 & 토큰이 없는가 등)
    /// </summary>
    /// <param name="isPosition"></param>
    /// <returns></returns>
    public bool IsGridEmpty(Vector2Int isPosition, bool enemyException = false, bool heroException = false, bool destructableExcpt = false)
    {
        if (enemyException || heroException)
            return grid.CanSetExceptionToken(isPosition, enemyException, heroException, destructableExcpt);
        else return grid.CanSet(isPosition);
    }
}
