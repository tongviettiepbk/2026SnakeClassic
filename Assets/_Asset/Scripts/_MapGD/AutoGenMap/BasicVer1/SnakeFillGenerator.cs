using System.Collections.Generic;
using UnityEngine;

public static class SnakeFillGenerator
{
    private static readonly Vector2Int[] DIRS =
    {
        new Vector2Int(1,0),
        new Vector2Int(-1,0),
        new Vector2Int(0,1),
        new Vector2Int(0,-1)
    };

    public static MapInfo Generate(int rows, int cols, float percent, int? seed = null)
    {
        System.Random rng = seed.HasValue ? new System.Random(seed.Value) : new System.Random();

        int total = rows * cols;
        int targetFill = Mathf.RoundToInt(total * percent);

        bool[,] used = new bool[cols, rows];

        MapInfo map = new MapInfo();
        map.row = rows;
        map.column = cols;
        map.listGeckoInMap = new List<GeckoDataInMap>();

        int filled = 0;
        int colorCounter = 0;

        while (filled < targetFill)
        {
            Vector2Int head = FindEmptyCell(used, cols, rows, rng);
            if (head.x < 0) break;

            List<Vector2Int> snake = GrowSnake(head, used, cols, rows, rng);

            if (snake.Count < 2)
            {
                used[head.x, head.y] = true;
                continue;
            }

            // Convert to index list
            List<int> nodeList = new List<int>();
            foreach (var p in snake) nodeList.Add(p.y * cols + p.x);

            // ================================
            // SELF–KILL CHECK + AUTO FIX
            // ================================
            if (SnakeSelfKillDetected(nodeList, cols))
            {
                TrimTailAfterHit(nodeList, cols);

                if (nodeList.Count < 2)
                    continue;
            }

            GeckoDataInMap g = new GeckoDataInMap();
            g.colorGecko = colorCounter++;
            g.listNode = nodeList;
            g.indexHead = nodeList[0];

            foreach (var idx in nodeList)
            {
                int x = idx % cols;
                int y = idx / cols;
                used[x, y] = true;
                filled++;
                if (filled >= targetFill) break;
            }

            map.listGeckoInMap.Add(g);
        }

        return map;
    }

    // ====================================================================
    // FIND EMPTY CELL
    // ====================================================================
    private static Vector2Int FindEmptyCell(bool[,] used, int cols, int rows, System.Random rng)
    {
        List<Vector2Int> empty = new List<Vector2Int>();

        for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
                if (!used[x, y])
                    empty.Add(new Vector2Int(x, y));

        if (empty.Count == 0)
            return new Vector2Int(-1, -1);

        return empty[rng.Next(empty.Count)];
    }

    // ====================================================================
    // GROW SNAKE RANDOM
    // ====================================================================
    private static List<Vector2Int> GrowSnake(
        Vector2Int head,
        bool[,] used,
        int cols,
        int rows,
        System.Random rng)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        path.Add(head);

        int maxLength = rng.Next(3, Mathf.Max(5, cols + rows));

        for (int i = 1; i < maxLength; i++)
        {
            Vector2Int cur = path[path.Count - 1];
            List<Vector2Int> cand = new List<Vector2Int>();

            foreach (var d in DIRS)
            {
                int nx = cur.x + d.x;
                int ny = cur.y + d.y;

                if (nx < 0 || nx >= cols) continue;
                if (ny < 0 || ny >= rows) continue;
                if (used[nx, ny]) continue;
                if (path.Contains(new Vector2Int(nx, ny))) continue;

                // simple quick grow
                cand.Add(new Vector2Int(nx, ny));
            }

            if (cand.Count == 0)
                break;

            var chosen = cand[rng.Next(cand.Count)];
            path.Add(chosen);
        }

        return path;
    }

    // ====================================================================
    // SELF-KILL DETECTION
    // ====================================================================
    private static bool SnakeSelfKillDetected(List<int> nodeList, int cols)
    {
        if (nodeList.Count < 3)
            return false;

        int head = nodeList[0];
        int next = nodeList[1];

        int hx = head % cols;
        int hy = head / cols;

        int nx = next % cols;
        int ny = next / cols;

        int dx = nx - hx;
        int dy = ny - hy;

        int targetX = hx + dx;
        int targetY = hy + dy;

        int targetIdx = targetY * cols + targetX;

        for (int i = 2; i < nodeList.Count; i++)
            if (nodeList[i] == targetIdx)
                return true;

        return false;
    }

    // ====================================================================
    // AUTO FIX: TRIM TAIL WHEN SELF-KILL
    // ====================================================================
    private static void TrimTailAfterHit(List<int> nodeList, int cols)
    {
        int head = nodeList[0];
        int next = nodeList[1];

        int hx = head % cols;
        int hy = head / cols;

        int nx = next % cols;
        int ny = next / cols;

        int dx = nx - hx;
        int dy = ny - hy;

        int targetX = hx + dx;
        int targetY = hy + dy;

        int targetIdx = targetY * cols + targetX;

        int hitPos = nodeList.IndexOf(targetIdx);

        if (hitPos > 1)
        {
            nodeList.RemoveRange(hitPos, nodeList.Count - hitPos);
        }
    }
}

