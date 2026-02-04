using System.Collections.Generic;
using UnityEngine;

public static class SnakeGeneratorBasicNonObstacle
{
    private static readonly Vector2Int[] DIRS =
    {
        new Vector2Int(1,0),
        new Vector2Int(-1,0),
        new Vector2Int(0,1),
        new Vector2Int(0,-1)
    };

    // ====================================================================
    // PUBLIC API
    // ====================================================================
    public static MapInfo Generate(
        int rows,
        int cols,
        float percentFill,
        int minLen,
        int maxLen,
        int? seed = null)
    {
        System.Random rng = seed.HasValue ? new System.Random(seed.Value) : new System.Random();

        int total = rows * cols;
        int targetFill = Mathf.RoundToInt(total * percentFill);

        bool[,] used = new bool[cols, rows];

        MapInfo map = new MapInfo();
        map.row = rows;
        map.column = cols;
        map.listGeckoInMap = new List<GeckoDataInMap>();

        int colorCounter = 0;
        int filled = 0;

        while (filled < targetFill)
        {
            // 1) Pick EXIT cell on border
            Vector2Int exitCell = PickBorderCell(used, cols, rows, rng);
            if (exitCell.x < 0) break;

            // 2) Grow snake INTO the map (this is escape path reversed)
            int snakeLen = rng.Next(minLen, maxLen + 1);
            List<Vector2Int> exitPath = GrowFromExit(exitCell, used, cols, rows, rng, snakeLen);

            if (exitPath.Count < 2)
            {
                used[exitCell.x, exitCell.y] = true;
                continue;
            }

            // 3) Reverse path = actual START path (solvable!)
            exitPath.Reverse();

            // 4) Convert to index list
            List<int> nodeList = new List<int>();
            foreach (var p in exitPath)
                nodeList.Add(p.y * cols + p.x);

            // 5) SELF-KILL DETECTION
            if (SnakeSelfKillDetected(nodeList, cols))
            {
                TrimTailAfterHit(nodeList, cols);

                if (nodeList.Count < 2)
                    continue;
            }

            // 6) Commit snake
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
            }

            map.listGeckoInMap.Add(g);
        }

        return map;
    }


    // ====================================================================
    // PICK EXIT ON BORDER
    // ====================================================================
    private static Vector2Int PickBorderCell(bool[,] used, int cols, int rows, System.Random rng)
    {
        List<Vector2Int> border = new List<Vector2Int>();

        for (int x = 0; x < cols; x++)
        {
            if (!used[x, 0]) border.Add(new Vector2Int(x, 0));
            if (!used[x, rows - 1]) border.Add(new Vector2Int(x, rows - 1));
        }
        for (int y = 0; y < rows; y++)
        {
            if (!used[0, y]) border.Add(new Vector2Int(0, y));
            if (!used[cols - 1, y]) border.Add(new Vector2Int(cols - 1, y));
        }

        if (border.Count == 0) return new Vector2Int(-1, -1);
        return border[rng.Next(border.Count)];
    }


    // ====================================================================
    // GROW FROM EXIT INTO MAP
    // ====================================================================
    private static List<Vector2Int> GrowFromExit(
        Vector2Int exitCell,
        bool[,] used,
        int cols,
        int rows,
        System.Random rng,
        int maxLen)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        path.Add(exitCell);

        for (int i = 1; i < maxLen; i++)
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

                Vector2Int next = new Vector2Int(nx, ny);

                // tránh tự quay lại
                if (path.Contains(next)) continue;

                cand.Add(next);
            }

            if (cand.Count == 0) break;

            var chosen = cand[rng.Next(cand.Count)];
            path.Add(chosen);
        }

        return path;
    }


    // ====================================================================
    // SELF-KILL DETECTION (CHUẨN — đơn giản — đúng logic bạn muốn)
    // ====================================================================
    private static bool SnakeSelfKillDetected(List<int> nodeList, int cols)
    {
        if (nodeList.Count < 3)
            return false;

        int head = nodeList[0];
        int neck = nodeList[1];

        int hx = head % cols;
        int hy = head / cols;

        int nx = neck % cols;
        int ny = neck / cols;

        // hướng thật sự của rắn
        int dx = nx - hx;
        int dy = ny - hy;

        int tx = hx + dx;
        int ty = hy + dy;

        int targetIdx = ty * cols + tx;

        // nếu targetIdx là 1 phần thân → tự đâm vào mình
        for (int i = 2; i < nodeList.Count; i++)
            if (nodeList[i] == targetIdx)
                return true;

        return false;
    }


    // ====================================================================
    // SELF-KILL FIX: TRIM TAIL
    // ====================================================================
    private static void TrimTailAfterHit(List<int> nodeList, int cols)
    {
        int head = nodeList[0];
        int neck = nodeList[1];

        int hx = head % cols;
        int hy = head / cols;

        int nx = neck % cols;
        int ny = neck / cols;

        int dx = nx - hx;
        int dy = ny - hy;

        int tx = hx + dx;
        int ty = hy + dy;
        int targetIdx = ty * cols + tx;

        int hitPos = nodeList.IndexOf(targetIdx);

        if (hitPos > 1)
            nodeList.RemoveRange(hitPos, nodeList.Count - hitPos);
    }
}
