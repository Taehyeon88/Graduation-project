using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class FindPathBFS
{
    static int[,] map;
    public static void SetGrid(TokenGirdCell[,] grid)    //플레이어 혹은 몬스터 각각의 이동 턴에 한번 실행
    {
        map = new int[grid.GetLength(0), grid.GetLength(1)];

        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                map[x, y] = grid[x, y].IsEmpty() ? 0 : 1;   //1 - 이동 불가 | 0 - 이동 가능
            }
        }

    }
    public static List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal)
    {
        if (map == null)
        {
            Debug.Log("map 이 없음");
        }
        map[start.x, start.y] = 0;
        var path = _FindPathBFS(map, start, goal);

        if(path != null)
        {
            Debug.Log($"경로 길이: {path.Count}");
            foreach (var p in path)
                Debug.Log(p);

            return path;
        }

        Debug.Log("경로 없음");
        return null;
    }

    static List<Vector2Int> _FindPathBFS(int[,] map, Vector2Int start, Vector2Int goal)
    {
        int w = map.GetLength(0);
        int h = map.GetLength(1);

        int[,] gCost = new int[w, h];                       //지금까지 온 최소 비용
        bool[,] visited = new bool[w, h];                  //확정 여부
        Vector2Int?[,] parent = new Vector2Int?[w, h];     //경로 복원용

        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                gCost[x, y] = int.MaxValue;

        gCost[start.x, start.y] = 0;

        Vector2Int[] dirs =
        {
            new Vector2Int(1,0), new Vector2Int(-1,0), new Vector2Int(0,1), new Vector2Int(0,-1),
            new Vector2Int(1,1), new Vector2Int(-1,1), new Vector2Int(-1,-1), new Vector2Int(1,-1)
        };

        List<Vector2Int> open = new List<Vector2Int>();
        open.Add(start);

        while (open.Count > 0)
        {
            int bestIndex = 0;
            int bestF = F(open[0], gCost, goal);
            for (int i = 1; i < open.Count; i++)
            {
                int f = F(open[i], gCost, goal);
                if (f < bestF)
                {
                    bestF = f;
                    bestIndex = i;
                }
            }
            Vector2Int cur = open[bestIndex];
            open.RemoveAt(bestIndex);

            if (visited[cur.x, cur.y]) continue;
            visited[cur.x, cur.y] = true;

            if (cur == goal) return ReconstructPath(parent, start, goal);

            foreach (var d in dirs)
            {
                int nx = cur.x + d.x;
                int ny = cur.y + d.y;

                if (!InBounds(map, nx, ny)) continue;
                if (map[nx, ny] == 1) continue;        //벽
                if (visited[nx, ny]) continue;

                int moveCost = TileCost(map[nx, ny]);  //cur -> (nx, my) 비용
                int newG = gCost[cur.x, cur.y] + moveCost;

                if (newG < gCost[nx, ny])
                {
                    gCost[nx, ny] = newG;
                    parent[nx, ny] = cur;

                    if (!open.Contains(new Vector2Int(nx, ny)))
                        open.Add(new Vector2Int(nx, ny));
                }
            }
        }
        return null;
    }

    static int F(Vector2Int pos, int[,] gCost, Vector2Int goal)
    {
        return gCost[pos.x, pos.y] + H(pos, goal);
    }

    static int H(Vector2Int a, Vector2Int b)
    {
        return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
    }

    static int TileCost(int tile)
    {
        return 0;
    }

    static bool InBounds(int[,] map, int x, int y)
    {
        return x >= 0 && y >= 0 &&
               x < map.GetLength(0) &&
               y < map.GetLength(1);
    }

    static List<Vector2Int> ReconstructPath(Vector2Int?[,] parent, Vector2Int start, Vector2Int goal)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int? cur = goal;

        while (cur.HasValue)
        {
            if (cur.Value == start) break;
            path.Add(cur.Value);
            cur = parent[cur.Value.x, cur.Value.y];
        }

        path.Reverse();
        return path;
    }
}
