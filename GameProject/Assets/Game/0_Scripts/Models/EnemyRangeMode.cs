using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyRangeMode
{
    public abstract List<Vector2Int> GetGridRanges(Vector2Int currentPosition, int distance = 1);
}

/// <summary>
/// 멘헤튼 거리식 - 인접 범위
/// </summary>
public class E_AllAroundRM : EnemyRangeMode
{
    public override List<Vector2Int> GetGridRanges(Vector2Int currentPosition, int distance = 1)
    {
        return TokenSystem.Instance.GetAllAroundPlaces(currentPosition, distance, false, true, true);
    }
}

/// <summary>
/// X자 직선식 범위
/// </summary>
public class E_CrossRM : EnemyRangeMode
{
    public override List<Vector2Int> GetGridRanges(Vector2Int currentPosition, int distance)
    {
        Vector2Int[] dirs = { new(0, 1), new(0, -1), new(1, 0), new(-1, 0) };
        List<Vector2Int> result = new();

        foreach (var dir in dirs)
        {
            for (int i = 1; i <= distance; i++)
            {
                Vector2Int position = currentPosition + dir * i;
                if (TokenSystem.Instance.IsGridEmpty(position, false, true, true))
                {
                    result.Add(position);
                }
                else break;
            }
        }
        return result;
    }
}

/// <summary>
/// 거리가 1칸짜리용 8방향 타일 범위
/// </summary>
public class E_AllEightAroundRM : EnemyRangeMode
{
    public override List<Vector2Int> GetGridRanges(Vector2Int currentPosition, int distance = 1)
    {
        Vector2Int[] dirs = { new(1, 0), new(0, 1), new(-1, 0), new(0, -1), new(1, 1), new(1, -1), new(-1, 1), new(-1, -1) };
        var result = new List<Vector2Int>();

        foreach (var dir in dirs)
        {
            Vector2Int targetPos = currentPosition + dir;
            if (!TokenSystem.Instance.IsGridEmpty(targetPos, false, true, true)) continue;

            result.Add(targetPos);
        }
        return result;
    }
}