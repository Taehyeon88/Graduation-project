using IsoTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TokenGrid : MonoBehaviour
{
    [SerializeField] private Transform gridTransform;
    [SerializeField] private Transform gridTilePool;
    [field: SerializeField] public int width { get; private set; }
    [field: SerializeField] public int height { get; private set; }
    public TokenGirdCell[,] grid { get; private set; }
    public int[,] simpleGrid { get; private set; }   //0 - 토큰 없음 | 1 - 토큰 있음 (토큰 존재여부 확인용)

    private List<Vector2Int> remainCells = new();

    private Dictionary<Vector2Int, IsoObject> gridTileByPos = new();

    private bool endInitialize = false;
    private void Start()
    {
        Initialize();
    }
    private void Initialize()
    {
        if (endInitialize) return;

        int index = 0;
        var temp = gridTransform.GetComponentsInChildren<IsoObject>(true);
        if (temp == null)
            Debug.LogError("gridTransform의 자식들에서 isoObject를 가지는 대상을 찾을 수 없습니다.");

        grid = new TokenGirdCell[width, height];
        simpleGrid = new int[width, height];
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                //가상 그리드 처리
                grid[x, y] = new();
                simpleGrid[x, y] = 0;
                remainCells.Add(new(x, y));

                //그리드 타일 처리
                IsoObject gridTile = temp[index];
                gridTile.gameObject.SetActive(true);
                gridTile.position = new Vector3(x, y, 0);

                Vector2Int gridPos = new(x, y);
                grid[gridPos.x, gridPos.y].SetField(gridTile);

                if (!gridTileByPos.TryAdd(gridPos, gridTile))
                    Debug.LogError($"{gridPos}위치에 {gridTileByPos[gridPos]}와 {gridTile}이 같은 위치로 충돌합니다.");

                index++;
            }
        }

        endInitialize = true;
    }

    public Vector2Int SetTokenRendomly(Token token)
    {
        if (grid == null) Initialize();

        int randomValue = UnityEngine.Random.Range(0, remainCells.Count);
        Vector2Int pos = remainCells[randomValue];
        grid[pos.x, pos.y].SetToken(token);
        simpleGrid[pos.x, pos.y] = 1;
        remainCells.Remove(pos);
        return pos;
    }
    public Vector2Int SetTokenRendomly(Token token, List<Vector2Int> canSetPositions)
    {
        if (grid == null) Initialize();

        int randomValue = UnityEngine.Random.Range(0, canSetPositions.Count);
        Vector2Int pos = canSetPositions[randomValue];
        grid[pos.x, pos.y].SetToken(token);
        simpleGrid[pos.x, pos.y] = 1;
        canSetPositions.Remove(pos);
        remainCells.Remove(pos);
        return pos;
    }

    public Vector2Int SetToken(Token token, Vector2Int pos)
    {
        if (grid == null) Initialize();

        grid[pos.x, pos.y].SetToken(token);
        simpleGrid[pos.x, pos.y] = 1;
        remainCells.Remove(pos);
        return pos;
    }
    public void ResetToken(Vector2Int pos)
    {
        grid[pos.x, pos.y].ResetToken();
        simpleGrid[pos.x, pos.y] = 0;
        remainCells.Add(pos);
    }
    public void SetField(IsoObject field, Vector2Int pos)
    {
        //기존의 필드가 기본 타일일 경우, pool로 이동 및 보관 / 아닐 경우, 파괴
        bool isDefualt = gridTileByPos[pos] == grid[pos.x, pos.y].field;
        if (isDefualt)
            grid[pos.x, pos.y].field.gameObject.transform.SetParent(gridTilePool);
        else
            Destroy(grid[pos.x, pos.y].field.gameObject);

        field.position = new(pos.x, pos.y, 0);
        field.transform.SetParent(gridTransform);
        grid[pos.x, pos.y].SetField(field);

    }
    public void ResetField(Vector2Int pos)
    {
        //기본 타일로 되돌리기
        Destroy(grid[pos.x, pos.y].field.gameObject);

        IsoObject field = gridTileByPos[pos];
        field.transform.SetParent(gridTransform);
        grid[pos.x, pos.y].SetField(field);

    }
    public void SetObject(IsoObject obj, Vector2Int pos)
    {
        if(grid[pos.x, pos.y].Object != null)
            Destroy(grid[pos.x, pos.y].Object.gameObject);

        obj.position = new(pos.x, pos.y, 1);
        obj.transform.SetParent(gridTransform);
        grid[pos.x, pos.y].SetObject(obj);
    }
    public void ResetObject(Vector2Int pos)
    {
        if (grid[pos.x, pos.y].Object != null)
        {
            Destroy(grid[pos.x, pos.y].Object.gameObject);
            grid[pos.x, pos.y].ResetObject();
        }
    }
    public bool IsBound(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height) return false;
        return true;
    }

    public bool CanSetByGridPos(Vector2Int pos)
    {
        int x = pos.x; int y = pos.y;
        if (x < 0 || x >= width || y < 0 || y >= height) return false;
        if (!grid[x, y].IsEmpty()) return false;
        return true;
    }
    public bool CanSetByGridPosExceptionToken(Vector2Int pos, bool exceptEnemy = false, bool exceptHero = false)
    {
        int x = pos.x; int y = pos.y;
        if (x < 0 || x >= width || y < 0 || y >= height) return false;
        if (!grid[x, y].IsEmpty())
        {
            if (exceptEnemy && grid[x, y].token is EnemyView) return true;
            else if (exceptHero && grid[x, y].token is HeroView) return true;
            else return false;
        }
        return true;
    }

    public List<Vector2Int> GetCanSetPositions(List<Vector2Int> positions = null)
    {
        if (grid == null) Initialize();

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
public class TokenGirdCell
{
    public Token token { get; private set; }
    public IsoObject field { get; private set; }
    public IsoObject Object { get; private set; }

    //토큰 관련 함수
    public void SetToken(Token token) => this.token = token;
    public void ResetToken() => this.token = null;
    public bool IsEmpty() => token == null;

    //필드 타일 관련 함수
    public void SetField(IsoObject field) => this.field = field;
    public void ResetField() => this.field = null;

    //오브젝트 관련 함수
    public void SetObject(IsoObject obj) => this.Object = obj;
    public void ResetObject() => this.Object = null;
}