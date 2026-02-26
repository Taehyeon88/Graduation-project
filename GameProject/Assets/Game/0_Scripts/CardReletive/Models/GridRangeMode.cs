using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class GridRangeMode
{
    public abstract List<Vector2Int> GetGridRanges(Vector2Int currentPosition, int distance, bool penetration);
}

public class AllAroundRM : GridRangeMode
{
    public override List<Vector2Int> GetGridRanges(Vector2Int currentPosition, int distance = 1, bool penetration = false)
    {
        int minX = currentPosition.x - distance; int minY = currentPosition.y - distance;
        int maxX = currentPosition.x + distance; int maxY = currentPosition.y + distance;

        List<Vector2Int> result = new();

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                Vector2Int position = new(x, y);
                if (TokenSystem.Instance.IsGridEmpty(position))
                {
                    result.Add(position);
                }
            }
        }
        return result;
    }
}

public class PlusRM : GridRangeMode
{
    public override List<Vector2Int> GetGridRanges(Vector2Int currentPosition, int distance = 1, bool penetration = false)
    {
        Vector2Int[] dirs = { new(1, 1), new(-1, -1), new(1, -1), new(-1, 1) };
        List<Vector2Int> result = new();

        foreach (var dir in dirs)
        {
            for (int i = 1; i <= distance; i++)
            {
                Vector2Int position = currentPosition + dir * i;
                if (TokenSystem.Instance.IsGridEmpty(position))
                {
                    result.Add(position);
                }
                else if (penetration) break;    //관통하면 다음 타일 무시
            }
        }
        return result;
    }
}

public class CrossRM : GridRangeMode
{
    public override List<Vector2Int> GetGridRanges(Vector2Int currentPosition, int distance, bool penetration = false)
    {
        Vector2Int[] dirs = { new(0, 1), new(0, -1), new(1, 0), new(-1, 0) };
        List<Vector2Int> result = new();

        foreach (var dir in dirs)
        {
            for (int i = 1; i <= distance; i++)
            {
                Vector2Int position = currentPosition + dir * i;
                if (TokenSystem.Instance.IsGridEmpty(position))
                {
                    result.Add(position);
                }
                else if (penetration) break;
            }
        }
        return result;
    }
}

public class SnowRM : GridRangeMode
{
    public override List<Vector2Int> GetGridRanges(Vector2Int currentPosition, int distance, bool penetration = false)
    {
        Vector2Int[] dirs = { new(0, 1), new(0, -1), new(1, 0), new(-1, 0), new(1, 1), new(-1, -1), new(1, -1), new(-1, 1) };
        List<Vector2Int> result = new();

        foreach (var dir in dirs)
        {
            for (int i = 1; i <= distance; i++)
            {
                Vector2Int position = currentPosition + dir * i;
                if (TokenSystem.Instance.IsGridEmpty(position))
                {
                    result.Add(position);
                }
                else if (penetration) break;
            }
        }
        return result;
    }
}