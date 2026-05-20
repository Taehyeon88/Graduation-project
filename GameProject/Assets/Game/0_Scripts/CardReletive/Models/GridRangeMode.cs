using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public abstract class GridRangeMode
{
    public abstract List<Vector2Int> GetGridRanges(Vector2Int currentPosition, int distance, bool penetration);
}

[System.Serializable]
public class AllAroundRM : GridRangeMode
{
    public override List<Vector2Int> GetGridRanges(Vector2Int currentPosition, int distance = 1, bool penetration = false)
    {
        return TokenSystem.Instance.GetAllAroundPlaces(currentPosition, distance, true);
    }
}

[System.Serializable]
public class AllAround_ExpceptEnemyRM : GridRangeMode
{
    public override List<Vector2Int> GetGridRanges(Vector2Int currentPosition, int distance = 1, bool penetration = false)
    {
        List<Vector2Int> result = new();
        for (int dis = 1; dis <= distance; dis++)
        {
            foreach (var dir in FindPathBFS.Dirs)
            {
                Vector2Int pos = currentPosition + dir * dis;
                if (TokenSystem.Instance.IsGridEmpty(pos))
                {
                    result.Add(pos);
                }
            }
        }
        return result;
    }
}

//[System.Serializable]
//public class PlusRM : GridRangeMode
//{
//    public override List<Vector2Int> GetGridRanges(Vector2Int currentPosition, int distance = 1, bool penetration = false)
//    {
//        Vector2Int[] dirs = { new(1, 1), new(-1, -1), new(1, -1), new(-1, 1) };
//        List<Vector2Int> result = new();

//        foreach (var dir in dirs)
//        {
//            for (int i = 1; i <= distance; i++)
//            {
//                Vector2Int position = currentPosition + dir * i;
//                if (TokenSystem.Instance.IsGridEmpty(position, true))
//                {
//                    result.Add(position);
//                }
//                else if (penetration) break;    //관통하면 다음 타일 무시
//            }
//        }
//        return result;
//    }
//}

[System.Serializable]
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
                if (TokenSystem.Instance.IsGridEmpty(position, true))
                {
                    result.Add(position);
                }
                else if (penetration) break;
            }
        }
        return result;
    }
}

//[System.Serializable]
//public class SnowRM : GridRangeMode
//{
//    public override List<Vector2Int> GetGridRanges(Vector2Int currentPosition, int distance, bool penetration = false)
//    {
//        Vector2Int[] dirs = { new(0, 1), new(0, -1), new(1, 0), new(-1, 0), new(1, 1), new(-1, -1), new(1, -1), new(-1, 1) };
//        List<Vector2Int> result = new();

//        foreach (var dir in dirs)
//        {
//            for (int i = 1; i <= distance; i++)
//            {
//                Vector2Int position = currentPosition + dir * i;
//                if (TokenSystem.Instance.IsGridEmpty(position, true))
//                {
//                    result.Add(position);
//                }
//                else if (penetration) break;
//            }
//        }
//        return result;
//    }
//}