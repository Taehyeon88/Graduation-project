using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public static class UtilityBFS
{
    public static Vector2Int[] Dirs { get; private set; } =
        {
            new Vector2Int(1,0), new Vector2Int(-1,0), new Vector2Int(0,1), new Vector2Int(0,-1)
        };

    //인접(거리) 범위 안에서 선택할 수 있는 모든 위치 받는 함수
    public static List<Vector2Int> FindAllPlaces(Vector2Int start, int maxDistance, bool exceptEnemy, bool exceptHero, bool exceptDestructable = false)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        List<Vector2Int> list = new List<Vector2Int>();
        bool[,] visited = new bool[TokenSystem.Instance.gridWidth, TokenSystem.Instance.gridHeight];

        queue.Enqueue(start);
        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            int dis = TokenSystem.Instance.GetDistance(start, current);
            if (dis > maxDistance) break;

            if (current != start)
                list.Add(current);

            visited[current.x, current.y] = true;

            foreach (var dir in Dirs)
            {
                int nx = current.x + dir.x;
                int ny = current.y + dir.y;
                Vector2Int target = new(nx, ny);

                if (!TokenSystem.Instance.IsBound(target)) continue;
                if (visited[nx, ny]) continue;

                if (!queue.Contains(target))
                    queue.Enqueue(target);
            }
        }

        foreach (var pos in list.ToList())
        {
            if (!TokenSystem.Instance.IsGridEmpty(pos, exceptEnemy, exceptHero, exceptDestructable))
            {
                list.Remove(pos);
            }
        }

        return list;
    }

    //인접(거리)까지의 범위 내에서 시작 위치에서 이어질 수 있는 모든 경로를 반는 함수
    public static List<Vector2Int> FindALLRoots(Vector2Int start, int maxDistance, bool exceptEnemy = false, bool exceptHero = false)
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        List<Vector2Int> list = new List<Vector2Int>();
        bool[,] visited = new bool[TokenSystem.Instance.gridWidth, TokenSystem.Instance.gridHeight];

        queue.Enqueue(start);
        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            int dis = TokenSystem.Instance.GetDistance(start, current);
            if (dis > maxDistance) break;

            if (current != start)
                list.Add(current);

            visited[current.x, current.y] = true;

            foreach (var dir in Dirs)
            {
                int nx = current.x + dir.x;
                int ny = current.y + dir.y;
                Vector2Int target = new(nx, ny);

                if (!TokenSystem.Instance.IsGridEmpty(new(nx,ny), exceptEnemy, exceptHero)) continue;
                if (visited[nx, ny]) continue;

                if(!queue.Contains(target))
                    queue.Enqueue(target);
            }
        }

        return list;
    }
    public static List<Vector2Int> FindShortestPath(int[,] map, Vector2Int start, Vector2Int goal)
    {
        map[start.x, start.y] = 0;

        int w = map.GetLength(0);
        int h = map.GetLength(1);

        int[,] gCost = new int[w, h];                       //지금까지 온 최소 비용
        bool[,] visited = new bool[w, h];                  //확정 여부
        Vector2Int?[,] parent = new Vector2Int?[w, h];     //경로 복원용

        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                gCost[x, y] = int.MaxValue;

        gCost[start.x, start.y] = 0;

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

            foreach (var d in Dirs)
            {
                int nx = cur.x + d.x;
                int ny = cur.y + d.y;

                if (!InBounds(map, nx, ny)) continue;
                if (map[nx, ny] == 1) continue;
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
