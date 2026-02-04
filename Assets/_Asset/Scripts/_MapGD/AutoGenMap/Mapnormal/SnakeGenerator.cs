using System.Collections.Generic;
using UnityEngine;

public static class SnakeGenerator
{
    private static readonly Vector2Int[] DIRS =
    {
        new Vector2Int(1,0),
        new Vector2Int(-1,0),
        new Vector2Int(0,1),
        new Vector2Int(0,-1)
    };

    public static MapInfo GenerateSnakes(
        int cols,
        int rows,
        float percentFill,
        int? seed = null,
        int minLen = 3,
        int maxLen = 20
    )
    {
        System.Random rng = seed.HasValue ? new System.Random(seed.Value) : new System.Random();

        int total = cols * rows;
        int targetCells = Mathf.RoundToInt(total * percentFill);

        bool[,] used = new bool[cols, rows];

        MapInfo map = new MapInfo();
        map.column = cols;
        map.row = rows;

        map.listGeckoInMap = new List<GeckoDataInMap>();
        map.listBrickInMap = new List<BrickDataInMap>();
        map.listStopInMap = new List<StopDataInMap>();
        map.listExitsInMap = new List<ExitHoleDataInMap>();
        map.listBarrierInMap = new List<BarrierDataInMap>();

        int filled = 0;
        int colorId = 0;

        while (filled < targetCells)
        {
            Vector2Int start = FindEmpty(used, cols, rows, rng);
            if (start.x < 0)
                break;

            List<Vector2Int> snakePath = RandomGrow(start, used, cols, rows, rng, minLen, maxLen);

            if (snakePath.Count < 2)
            {
                used[start.x, start.y] = true;
                continue;
            }

            // Convert to index
            List<int> nodeList = new List<int>();
            foreach (var p in snakePath)
                nodeList.Add(p.y * cols + p.x);

            // ======================
            // SELF–KILL CHECK
            // ======================
            if (SnakeSelfKillDetected(nodeList, cols))
            {
                TrimTailAfterHit(nodeList, cols);

                if (nodeList.Count < 2)
                    continue;
            }

            GeckoDataInMap g = new GeckoDataInMap();
            g.colorGecko = colorId++;
            g.listNode = nodeList;
            g.indexHead = nodeList[0];

            // commit to used[]
            foreach (var idx in nodeList)
            {
                int x = idx % cols;
                int y = idx / cols;
                used[x, y] = true;
                filled++;
            }

            map.listGeckoInMap.Add(g);
        }

        return map;
    }

    // ===========================================================
    // RANDOM GROW
    // ===========================================================

    private static Vector2Int FindEmpty(bool[,] used, int cols, int rows, System.Random rng)
    {
        List<Vector2Int> list = new List<Vector2Int>();

        for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
                if (!used[x, y])
                    list.Add(new Vector2Int(x, y));

        if (list.Count == 0)
            return new Vector2Int(-1, -1);

        return list[rng.Next(list.Count)];
    }

    private static List<Vector2Int> RandomGrow(
        Vector2Int start,
        bool[,] used,
        int cols,
        int rows,
        System.Random rng,
        int minLen,
        int maxLen)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        path.Add(start);

        int targetLen = rng.Next(minLen, maxLen);

        for (int i = 1; i < targetLen; i++)
        {
            Vector2Int cur = path[path.Count - 1];

            List<Vector2Int> cand = new List<Vector2Int>();

            foreach (var d in DIRS)
            {
                int nx = cur.x + d.x;
                int ny = cur.y + d.y;

                if (nx < 0 || nx >= cols || ny < 0 || ny >= rows)
                    continue;

                if (used[nx, ny]) continue;

                Vector2Int next = new Vector2Int(nx, ny);

                if (path.Contains(next)) continue;

                cand.Add(next);
            }

            if (cand.Count == 0)
                break;

            var chosen = cand[rng.Next(cand.Count)];
            path.Add(chosen);
        }

        return path;
    }

    // ===========================================================
    // SELF-KILL DETECTION (simple & fast)
    // ===========================================================

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

        if (targetX < 0 || targetY < 0)
            return false;

        int targetIdx = targetY * cols + targetX;

        for (int i = 2; i < nodeList.Count; i++)
        {
            if (nodeList[i] == targetIdx)
                return true;
        }

        return false;
    }

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
