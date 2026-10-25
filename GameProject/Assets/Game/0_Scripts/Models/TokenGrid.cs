using IsoTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TokenGrid : MonoBehaviour
{
    [Header("GridSettings")]
    [SerializeField] private Transform gridTransform;
    [SerializeField] private Transform gridTilePool;
    [SerializeField] private IsoObject sideTilePrefab;
    [SerializeField] private Vector3 heroSideTilePosition;
    [SerializeField] private Vector3 enemySideTilePosition;
    [field: SerializeField] public int width { get; private set; }
    [field: SerializeField] public int height { get; private set; }

    [Header("NeutralTile (비어 있으면 중립지대 타일을 숨김)")]
    [SerializeField] private Sprite[] neutralTileSprites;

    //외부 접근용
    public Dictionary<Token, Vector2Int> gridPosByToken = new();
    public Dictionary<Vector2Int, Token> tokenByGridPos = new();
    public int[,] simpleGrid { get; private set; }   //0 - 토큰 없음 | 1 - 토큰 있음 (토큰 존재여부 확인용)

    private List<Vector2Int> remainCells = new();
    private readonly HashSet<Vector2Int> neutralCells = new();   //중립지대 칸 (토큰이 올라갔다 떠나도 막힌 채로 유지)
    private IsoObject[,] floorTiles;   //바닥 타일 (중립지대 타일 교체·숨김용)
    private readonly Dictionary<SpriteRenderer, Sprite> originalSprites = new();   //중립지대로 교체한 타일의 원래 스프라이트

    /// <summary>
    /// 스테이지 바닥 그리드 생성 및 초기화 함수
    /// </summary>
    public void GenerateStage()
    {
        IsoObject[] temp = gridTransform.GetComponentsInChildren<IsoObject>(true);
        if (temp == null)
            Debug.LogError("gridTransform의 자식들에서 isoObject를 가지는 대상을 찾을 수 없습니다.");

        simpleGrid = new int[width, height];
        floorTiles = new IsoObject[width, height];
        remainCells = new();
        neutralCells.Clear();

        int index = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                simpleGrid[x, y] = 0;
                remainCells.Add(new(x, y));

                //gridTilePool에서 가져와서 배치
                IsoObject gridTile = temp[index];
                gridTile.gameObject.SetActive(true);
                RestoreTileSprite(gridTile);
                gridTile.position = new Vector3(x, y, 0);
                floorTiles[x, y] = gridTile;

                index++;
            }
        }

        CreateSideTile(heroSideTilePosition);
        CreateSideTile(enemySideTilePosition);
    }

    //그리드 양옆(아군/적 진영) 타일 생성
    private void CreateSideTile(Vector3 isoPosition)
    {
        if (sideTilePrefab == null) return;   //프리팹이 비어 있으면 사이드 타일 없이 생성 (주점 등)

        IsoObject sideTile = Instantiate(sideTilePrefab, TokenSystem.Instance.IsoWorld.transform);
        sideTile.position = isoPosition;
    }

    //중립지대 등록 (토큰 없이 칸만 막고 바닥 타일은 랜덤 중립 스프라이트로 교체, 스프라이트 미할당 시 숨김)
    public void SetBlockedCell(Vector2Int pos)
    {
        if (!IsBound(pos.x, pos.y)) return;
        simpleGrid[pos.x, pos.y] = 1;
        remainCells.Remove(pos);
        neutralCells.Add(pos);

        IsoObject floorTile = floorTiles[pos.x, pos.y];
        SpriteRenderer renderer = floorTile.GetComponentInChildren<SpriteRenderer>(true);
        if (neutralTileSprites == null || neutralTileSprites.Length == 0 || renderer == null)
        {
            floorTile.gameObject.SetActive(false);
            return;
        }

        originalSprites.TryAdd(renderer, renderer.sprite);
        renderer.sprite = neutralTileSprites[Random.Range(0, neutralTileSprites.Length)];
    }

    //중립지대로 교체했던 타일을 원래 스프라이트로 복원 (스테이지 재생성 시)
    private void RestoreTileSprite(IsoObject floorTile)
    {
        SpriteRenderer renderer = floorTile.GetComponentInChildren<SpriteRenderer>(true);
        if (renderer != null && originalSprites.Remove(renderer, out Sprite original))
            renderer.sprite = original;
    }

    public void SetToken(Token token, Vector2Int pos)
    {
        gridPosByToken.Add(token, pos);
        tokenByGridPos.Add(pos, token);
        simpleGrid[pos.x, pos.y] = 1;
        remainCells.Remove(pos);
    }
    public void RemoveToken(Token token)
    {
        Vector2Int pos = gridPosByToken[token];

        gridPosByToken.Remove(token);
        tokenByGridPos.Remove(pos);
        FreeCell(pos);
    }

    //토큰이 떠난 칸 비우기 (중립지대는 막힌 채로 유지)
    private void FreeCell(Vector2Int pos)
    {
        if (neutralCells.Contains(pos)) return;

        simpleGrid[pos.x, pos.y] = 0;
        remainCells.Add(pos);
    }
    public void ChangeTokenPos(Token token, Vector2Int targetPos)
    {
        Vector2Int pos = gridPosByToken[token];
        FreeCell(pos);
        tokenByGridPos.Remove(pos);

        simpleGrid[targetPos.x, targetPos.y] = 1;
        remainCells.Remove(targetPos);

        gridPosByToken[token] = targetPos;
        tokenByGridPos[targetPos] = token;
    }

    public int[,] GetSimpleGridCopied()
    {
        return (int[,])simpleGrid.Clone();
    }

    //public void SetObject(IsoObject obj, Vector2Int pos)
    //{
    //    if(grid[pos.x, pos.y].Object != null)
    //        Destroy(grid[pos.x, pos.y].Object.gameObject);

    //    obj.position = new(pos.x, pos.y, 1);
    //    obj.transform.SetParent(gridTransform);
    //    grid[pos.x, pos.y].SetObject(obj);
    //}
    //public void ResetObject(Vector2Int pos)
    //{
    //    if (grid[pos.x, pos.y].Object != null)
    //    {
    //        Destroy(grid[pos.x, pos.y].Object.gameObject);
    //        grid[pos.x, pos.y].ResetObject();
    //    }
    //}
    public bool IsBound(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return false;
        return true;
    }

    public bool CanSet(Vector2Int pos)
    {
        int x = pos.x; int y = pos.y;
        if (x < 0 || x >= width || y < 0 || y >= height) return false;
        if (simpleGrid[x, y] == 1) return false;
        return true;
    }
    /// <summary>
    /// 특정 토큰을 예외로 하고 설치 가능 여부 판단
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="exceptEnemy"></param>
    /// <param name="exceptHero"></param>
    /// <param name="exceptDestructable"></param>
    /// <returns></returns>
    public bool CanSetExceptionToken(Vector2Int pos, bool exceptEnemy = false, bool exceptHero = false, bool exceptDestructable = false)
    {
        int x = pos.x; int y = pos.y;
        if (x < 0 || x >= width || y < 0 || y >= height) return false;
        if (simpleGrid[x, y] == 1)
        {
            Token token = TokenSystem.Instance.API.GetTokenByPosition(pos);
            if (token == null) return false;   //토큰 없이 막힌 칸(중립지대)
            if (!exceptEnemy && token is EnemyView) return false;
            if (!exceptHero && token is HeroView) return false;
        }
        return true;
    }

    public List<Vector2Int> GetCanSetPositions(List<Vector2Int> positions = null)
    {
        if (positions != null)
        {
            int[,] temp = new int[width, height];
            foreach (Vector2Int pos in positions)
                temp[pos.x, pos.y] += 1;
            foreach (Vector2Int pos in remainCells)
                temp[pos.x, pos.y] += 1;
            positions.Clear();
            for (int x = 0; x < temp.GetLength(0); x++)
            {
                for (int y = 0; y < temp.GetLength(1); y++)
                {
                    if (temp[x, y] == 2)
                        positions.Add(new(x, y));
                }
            }
            return positions;
        }
        return null;
    }
}