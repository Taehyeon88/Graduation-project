using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyTargetMode
{
    public abstract List<Vector2Int> GetDirections(List<Vector2Int> range, Vector2Int targetPos, Vector2Int currentPos, int distance);
}

/// <summary>
/// 타겟 위치의 한 칸의 방향값 전달
/// </summary>
public class E_SingleTM : EnemyTargetMode
{
    public override List<Vector2Int> GetDirections(List<Vector2Int> range, Vector2Int targetPos, Vector2Int currentPos, int distance)
    {
        if (range.Contains(targetPos)) return new() { targetPos - currentPos };
        else return null;
    }
}


/// <summary>
/// 몬스터를 기준으로 직선 방향의 모든 방향값들 전달
/// </summary>
public class E_LineTM : EnemyTargetMode
{
    public override List<Vector2Int> GetDirections(List<Vector2Int> range, Vector2Int targetPos, Vector2Int currentPos, int distance)
    {
        if (range.Contains(targetPos))
        {
            Vector2Int direction = Utility.GetSignVector2Int(targetPos - currentPos);
            List<Vector2Int> result = new();

            for (int i = 1; i <= distance; i++)
            {
                Vector2Int dirs = direction * i;
                if (range.Contains(currentPos + dirs))
                {
                    result.Add(dirs);
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

/// <summary>
/// 특정 방향 라인의 방향값만 전달
/// </summary>
public class E_LineOneTM : EnemyTargetMode
{
    public override List<Vector2Int> GetDirections(List<Vector2Int> range, Vector2Int targetPos, Vector2Int currentPos, int distance)
    {
        if (range.Contains(targetPos))
        {
            Vector2Int dir = Utility.GetSignVector2Int(targetPos - currentPos);
            List<Vector2Int> result = new();

            result.Add(dir);
            return result;
        }
        else
        {
            return null;
        }
    }
}

/// <summary>
/// 받은 모든 범위의 방향값들을 반환
/// </summary>
public class E_GlobalTM : EnemyTargetMode
{
    public override List<Vector2Int> GetDirections(List<Vector2Int> range, Vector2Int targetPos, Vector2Int currentPos, int distance)
    {
        if (range.Contains(targetPos))
        {
            var result = new List<Vector2Int>();
            foreach (Vector2Int pos in range)
            {
                Vector2Int dir = pos - currentPos;
                result.Add(dir);
            }
            return result;
        }
        else return null;
    }
}

/// <summary>
/// ㄴ자로 횡베기 방향값들 반환 (8방향 범위 필요)
/// </summary>
public class E_ConeTM : EnemyTargetMode
{
    public override List<Vector2Int> GetDirections(List<Vector2Int> range, Vector2Int targetPos, Vector2Int currentPos, int distance)
    {
        if (range.Contains(targetPos))
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
                    result.Add(r - currentPos);
            }
            return result;
        }
        else
        {
            return null;
        }
    }
}
