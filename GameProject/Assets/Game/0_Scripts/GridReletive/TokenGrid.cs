using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TokenGrid : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    public TokenGirdCell[,] grid { get; private set; }

    private List<Vector2Int> remainCells = new();

    private bool endInitialize = false;
    private void Start()
    {
        Initialize();
    }
    private void Initialize()
    {
        if (endInitialize) return;

        grid = new TokenGirdCell[width, height];
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                grid[x, y] = new();
                remainCells.Add(new(x, y));
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
        remainCells.Remove(pos);
        return pos;
    }
    public Vector2Int SetToken(Token token, Vector3 position)
    {
        Vector2Int pos = WorldToGirdPosition(position);
        grid[pos.x, pos.y].SetToken(token);
        remainCells.Remove(pos);
        return pos;
    }
    public void ResetToken(Vector2Int pos)
    {
        grid[pos.x, pos.y].ResetToken();
        remainCells.Add(pos);
    }
    public bool CanSet(Vector3 position)
    {
        Vector2Int pos = WorldToGirdPosition(position);
        int x = pos.x; int y = pos.y;
        if (x < 0 || x >= width || y < 0 || y >= height) return false;
        if (!grid[x, y].IsEmpty()) return false;
        return true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (TokenSystem.CellSize <= 0 || width <= 0 || height <= 0) return;
        Vector3 origin = transform.position;
        for (int y = 0; y <= height; y++)
        {
            Vector3 start = origin + new Vector3(0, 0.01f, y * TokenSystem.CellSize);
            Vector3 end = origin + new Vector3(width * TokenSystem.CellSize, 0.01f, y * TokenSystem.CellSize);
            Gizmos.DrawLine(start, end);
        }
        for (int x = 0; x <= width; x++)
        {
            Vector3 start = origin + new Vector3(x * TokenSystem.CellSize, 0.01f, 0);
            Vector3 end = origin + new Vector3(x * TokenSystem.CellSize, 0.01f, height * TokenSystem.CellSize);
            Gizmos.DrawLine(start, end);
        }
    }

    public Vector2Int WorldToGirdPosition(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt((worldPosition - transform.position).x / TokenSystem.CellSize);
        int y = Mathf.FloorToInt((worldPosition - transform.position).z / TokenSystem.CellSize);
        return new(x, y);
    }
    public Vector3 GridToWorldPosition(Vector2Int gridPosition)
    {
        float x = transform.position.x + gridPosition.x * TokenSystem.CellSize + TokenSystem.CellSize / 2f;
        float z = transform.position.z + gridPosition.y * TokenSystem.CellSize + TokenSystem.CellSize / 2f;
        return new(x, 0f, z);
    }
}
public class TokenGirdCell
{
    public Token token { get; private set; }
    public void SetToken(Token token)
    {
        this.token = token;
    }
    public void ResetToken()
    {
        this.token = null;
    }
    public bool IsEmpty()
    {
        return token == null;
    }
}