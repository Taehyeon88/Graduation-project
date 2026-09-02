using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRangeGetor
{
    private static Vector2Int MyPos;
    private static int Distance;

    public static void SetInfo(Vector2Int myPos, int distance)
    {
        MyPos = myPos;
        Distance = distance;
    }
    public static List<Vector2Int> GetRanges(ERangeModeType R_Type)
    {
        switch(R_Type)
        {
            case ERangeModeType.BASIC:
                return TokenSystem.Instance.API.GetAllAroundPlaces(MyPos, Distance, false, true);

            case ERangeModeType.CROSS:
                Vector2Int[] dirs = { new(0, 1), new(0, -1), new(1, 0), new(-1, 0) };
                List<Vector2Int> result = new(30);

                foreach (var dir in dirs)
                {
                    for (int i = 1; i <= Distance; i++)
                    {
                        Vector2Int position = MyPos + dir * i;
                        if (TokenSystem.Instance.API.IsGridEmpty(position, false, true))
                            result.Add(position);
                        else break;
                    }
                }
                return result;

            case ERangeModeType.EIGHT:
                Vector2Int[] dirs2 = { new(1, 0), new(0, 1), new(-1, 0), new(0, -1), new(1, 1), new(1, -1), new(-1, 1), new(-1, -1) };
                var result2 = new List<Vector2Int>(8);

                foreach (var dir in dirs2)
                {
                    Vector2Int targetPos = MyPos + dir;
                    if (!TokenSystem.Instance.API.IsGridEmpty(targetPos, false, true)) continue;
                    result2.Add(targetPos);
                }
                return result2;

            default:
                return null;
        }
    }
}
