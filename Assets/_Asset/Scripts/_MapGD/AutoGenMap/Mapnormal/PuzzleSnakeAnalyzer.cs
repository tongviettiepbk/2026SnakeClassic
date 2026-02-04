using System.Collections.Generic;
using UnityEngine;

public static class PuzzleSnakeAnalyzer
{
    private static readonly Vector2Int[] DIRS = new Vector2Int[]
    {
        new Vector2Int(1,0),
        new Vector2Int(-1,0),
        new Vector2Int(0,1),
        new Vector2Int(0,-1)
    };

    // Build dependency graph: snake A depends on snake B blocking its escape
    public static Dictionary<int, List<int>> BuildDependency(MapInfo map)
    {
        int cols = map.column;
        int rows = map.row;

        int totalSnakes = map.listGeckoInMap.Count;

        // occupancy
        int[] occ = new int[cols * rows];
        for (int i = 0; i < occ.Length; i++) occ[i] = -1;

        for (int g = 0; g < totalSnakes; g++)
            foreach (int n in map.listGeckoInMap[g].listNode)
                occ[n] = g;

        var dep = new Dictionary<int, List<int>>();
        for (int g = 0; g < totalSnakes; g++)
            dep[g] = new List<int>();

        // check dependencies
        for (int g = 0; g < totalSnakes; g++)
        {
            var snake = map.listGeckoInMap[g];
            int head = snake.indexHead;

            int hx = head % cols;
            int hy = head / cols;

            foreach (var d in DIRS)
            {
                int nx = hx + d.x;
                int ny = hy + d.y;

                // Hướng ra map?
                while (true)
                {
                    if (nx < 0 || nx >= cols || ny < 0 || ny >= rows)
                        break;

                    int idx = ny * cols + nx;

                    if (occ[idx] != -1 && occ[idx] != g)
                    {
                        int blocker = occ[idx];
                        if (!dep[g].Contains(blocker))
                            dep[g].Add(blocker);
                        break;
                    }

                    nx += d.x;
                    ny += d.y;
                }
            }
        }

        return dep;
    }

    // Check if a single snake can escape by itself
    public static bool SnakeCanEscapeSelf(GeckoDataInMap gecko, int cols, int rows)
    {
        int head = gecko.indexHead;
        int hx = head % cols;
        int hy = head / cols;

        HashSet<int> body = new HashSet<int>(gecko.listNode);

        Queue<(int x, int y)> q = new Queue<(int, int)>();
        HashSet<int> visited = new HashSet<int>();

        q.Enqueue((hx, hy));
        visited.Add(head);

        Vector2Int[] dirs = new Vector2Int[]
        {
        new Vector2Int(1,0),
        new Vector2Int(-1,0),
        new Vector2Int(0,1),
        new Vector2Int(0,-1)
        };

        while (q.Count > 0)
        {
            var (x, y) = q.Dequeue();

            // If any reachable point hits boundary → snake can escape
            if (x == 0 || x == cols - 1 || y == 0 || y == rows - 1)
                return true;

            foreach (var d in dirs)
            {
                int nx = x + d.x;
                int ny = y + d.y;

                if (nx < 0 || nx >= cols || ny < 0 || ny >= rows)
                    continue;

                int idx = ny * cols + nx;

                // cannot BFS through own body
                if (body.Contains(idx))
                    continue;

                if (visited.Contains(idx))
                    continue;

                visited.Add(idx);
                q.Enqueue((nx, ny));
            }
        }

        return false; // no path reaches boundary → self-block
    }

}
