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
        int minX = currentPosition.x - distance; int minY = currentPosition.y - distance;
        int maxX = currentPosition.x + distance; int maxY = currentPosition.y + distance;

        List<Vector2Int> result = new();

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                Vector2Int position = new(x, y);
                if (TokenSystem.Instance.IsGridEmpty(position, true))
                {
                    result.Add(position);
                }
            }
        }
        return result;
    }
}
