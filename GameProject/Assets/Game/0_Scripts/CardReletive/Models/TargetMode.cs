using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class TargetMode
{
    public abstract List<Vector2Int> GetTargets(List<Vector2Int> range, Vector2Int targetPos, Vector2Int currentPos, int distance);
}

[System.Serializable]
public class SingleTM : TargetMode
{
    public override List<Vector2Int> GetTargets(List<Vector2Int> range, Vector2Int targetPos, Vector2Int currentPos, int distance)
    {
        if (range.Contains(targetPos)) return new() { targetPos };
        else return null;
    }
}

[System.Serializable]
public class LineTM : TargetMode
{
    public override List<Vector2Int> GetTargets(List<Vector2Int> range, Vector2Int targetPos, Vector2Int currentPos, int distance)
    {
        if (range.Contains(targetPos))
        {
            int dirX = (targetPos - currentPos).x != 0 ? (targetPos - currentPos).x / Mathf.Abs((targetPos - currentPos).x) : 0;
            int dirY = (targetPos - currentPos).y != 0 ? (targetPos - currentPos).y / Mathf.Abs((targetPos - currentPos).y) : 0;

            Vector2Int dir = new(dirX, dirY);
            List<Vector2Int> result = new();

            for (int i = 1; i <= distance; i++)
            {
                Vector2Int position = currentPos + dir * i;
                if (range.Contains(position))
                {
                    result.Add(position);
                }
            }
            return result;
        }
        else
        {
            return null;
        }
    }
}

[System.Serializable]
public class ConeTM : TargetMode
{
    public override List<Vector2Int> GetTargets(List<Vector2Int> range, Vector2Int targetPos, Vector2Int currentPos, int distance)
    {
        if (range.Contains(targetPos) && distance == 1)
        {
            Vector2Int dir = (targetPos - currentPos);
            List<Vector2Int> result = new();

            //예시 : 1,1 <- 1,0 | 1,-1 <- 0,-1 | -1,-1 <- -1,0 | -1,1 <- 0,1
            if (dir == new Vector2Int(1, 0)) dir = new(1, 1);
            else if (dir == new Vector2Int(0, -1)) dir = new(1, -1);
            else if (dir == new Vector2Int(-1, 0)) dir = new(-1, -1);
            else if (dir == new Vector2Int(0, 1)) dir = new(-1, 1);

            Vector2Int target = currentPos + dir;

            foreach (var r in range)
            {
                int dis = Mathf.Max(Mathf.Abs(r.x - target.x), Mathf.Abs(r.y - target.y));
                if (dis <= 1)
                {
                    result.Add(r);
                }
            }
            return result;
        }
        else
        {
            return null;
        }
    }
}

[System.Serializable]
public class GlobalTM : TargetMode
{
    public override List<Vector2Int> GetTargets(List<Vector2Int> range, Vector2Int targetPos, Vector2Int currentPos, int distance)
    {
        if (range.Contains(targetPos)) return new(range);
        else return null;
    }
}

public enum GridTargetingType
{
    Single,     //단일 
    Line,       //관통
    Cone,       //횡베기
    Radius,     //폭발
    Global      //전방위
}