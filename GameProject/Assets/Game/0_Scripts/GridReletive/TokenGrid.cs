using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TokenGrid : MonoBehaviour
{
    [SerializeField] private int width;
    [SerializeField] private int height;
    private TokenGirdCell[,] grid;

    private void Start()
    {
        grid = new TokenGirdCell[width, height];
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for(int y = 0; y < grid.GetLength(1); y++)
            {
                grid[x, y] = new();
            }
        }
    }

    public void SetToken(Token token, List<Vector3> allTokenPositions)
    {
        foreach (var p in allTokenPositions)
        {
            (int x, int y) = WorldToGirdPosition(p);
            grid[x, y].SetToken(token);
        }
    }

    public bool CanSet(List<Vector3> allTokenPositions)
    {
        foreach (var p in allTokenPositions)
        {
            (int x, int y) = WorldToGirdPosition(p);
            if (x < 0 || x >= width || y < 0 || y >= height) return false;
            if (!grid[x, y].IsEmpty()) return false;
        }
        return true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (TokenManager.CellSize <= 0 || width <= 0 || height <= 0) return;
        Vector3 origin = transform.position;
        for (int y = 0; y <= height; y++)
        {
            Vector3 start = origin + new Vector3(0, 0.01f, y * TokenManager.CellSize);
            Vector3 end = origin + new Vector3(width * TokenManager.CellSize, 0.01f, y * TokenManager.CellSize);
            Gizmos.DrawLine(start, end);
        }
        for (int x = 0; x <= width; x++)
        {
            Vector3 start = origin + new Vector3(x * TokenManager.CellSize, 0.01f, 0);
            Vector3 end = origin + new Vector3(x * TokenManager.CellSize, 0.01f, height * TokenManager.CellSize);
            Gizmos.DrawLine(start, end);
        }
    }

    private (int x, int y) WorldToGirdPosition(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt((worldPosition - transform.position).x / TokenManager.CellSize);
        int y = Mathf.FloorToInt((worldPosition - transform.position).z / TokenManager.CellSize);
        return (x, y);
    }
}
public class TokenGirdCell
{
    private Token token;
    public void SetToken(Token token)
    {
        this.token = token;
    }
    public bool IsEmpty()
    {
        return token == null;
    }
}