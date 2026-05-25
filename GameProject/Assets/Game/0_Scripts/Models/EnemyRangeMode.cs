using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyRangeMode
{
    public abstract List<Vector2Int> GetGridRanges(Vector2Int currentPosition, int distance = 1, bool penetration = false);
}

public class E_AllAroundRM : EnemyRangeMode
{
    public override List<Vector2Int> GetGridRanges(Vector2Int currentPosition, int distance = 1, bool penetration = false)
    {
        return TokenSystem.Instance.GetAllAroundPlaces(currentPosition, distance, false, true, true);
    }
}

public class E_SnowRM : EnemyRangeMode
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
                if (TokenSystem.Instance.IsGridEmpty(position, true, true))
                {
                    result.Add(position);
                }
                else if (penetration) break;
            }
        }
        return result;
    }
}