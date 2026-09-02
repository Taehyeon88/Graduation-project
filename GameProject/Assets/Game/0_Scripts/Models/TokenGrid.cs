using IsoTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TokenGrid : MonoBehaviour
{
    [Header("GridSettings")]
    [SerializeField] private Transform gridTransform;
    [SerializeField] private Transform gridTilePool;
    [field: SerializeField] public int width { get; private set; }
    [field: SerializeField] public int height { get; private set; }

    //외부 접근용
    public Dictionary<Token, Vector2Int> gridPosByToken = new();
    public Dictionary<Vector2Int, Token> tokenByGridPos = new();
    public int[,] simpleGrid { get; private set; }   //0 - 토큰 없음 | 1 - 토큰 있음 (토큰 존재여부 확인용)

    private List<Vector2Int> remainCells = new();

    /// <summary>
    /// 스테이지 바닥 그리드 생성 및 초기화 함수
    /// </summary>
    public void GenerateStage()
    {
        IsoObject[] temp = gridTransform.GetComponentsInChildren<IsoObject>(true);
        if (temp == null)
            Debug.LogError("gridTransform의 자식들에서 isoObject를 가지는 대상을 찾을 수 없습니다.");

        simpleGrid = new int[width, height];
        remainCells = new();

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
                gridTile.position = new Vector3(x, y, 0);

                index++;
            }
        }
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
        simpleGrid[pos.x, pos.y] = 0;
        remainCells.Add(pos);
    }
    public void ChangeTokenPos(Token token, Vector2Int targetPos)
    {
        Vector2Int pos = gridPosByToken[token];
        remainCells.Add(pos);
        simpleGrid[pos.x, pos.y] = 0;
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