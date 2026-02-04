using System.Collections.Generic;
using UnityEngine;

public static class SnakeMapGenerator_NoSelfKill_And_FilterEscape
{
    private static readonly Vector2Int[] DIRS =
    {
        new Vector2Int(1,0),
        new Vector2Int(-1,0),
        new Vector2Int(0,1),
        new Vector2Int(0,-1)
    };

    // ============================================================
    // PUBLIC API
    // ============================================================
    public static MapInfo Generate(
        int rows,
        int cols,
        float percentFill,
        int minLen,
        int maxLen,
        int? seed = null)
    {
        System.Random rng = seed == null ? new System.Random() : new System.Random(seed.Value);

        int total = rows * cols;
        int targetFill = Mathf.RoundToInt(total * percentFill);

        bool[,] used = new bool[cols, rows];

        MapInfo map = new MapInfo();
        map.row = rows;
        map.column = cols;
        map.listGeckoInMap = new List<GeckoDataInMap>();

        int filled = 0;
        int color = 0;

        while (filled < targetFill)
        {
            Vector2Int exit = PickBorder(rows, cols, used, rng);
            if (exit.x < 0) break;

            List<Vector2Int> path = GrowFromExit_Safe(exit, used, rows, cols, rng, minLen, maxLen);

            if (path.Count < 2)
            {
                used[exit.x, exit.y] = true;
                continue;
            }

            // convert path -> indices
            List<int> nodes = new List<int>();
            foreach (var p in path)
                nodes.Add(p.y * cols + p.x);

            GeckoDataInMap g = new GeckoDataInMap();
            g.colorGecko = color++;
            g.indexHead = nodes[0];
            g.listNode = nodes;

            foreach (var idx in nodes)
            {
                int x = idx % cols;
                int y = idx / cols;
                used[x, y] = true;
                filled++;
            }

            map.listGeckoInMap.Add(g);
        }

        // STEP 2: loại rắn không thoát
        FilterSnakes_CannotEscape(map, rows, cols);

        return map;
    }

    // ============================================================
    // PICK EXIT BORDER CELL
    // ============================================================
    private static Vector2Int PickBorder(int rows, int cols, bool[,] used, System.Random rng)
    {
        List<Vector2Int> lst = new List<Vector2Int>();

        for (int x = 0; x < cols; x++)
        {
            if (!used[x, 0]) lst.Add(new Vector2Int(x, 0));
            if (!used[x, rows - 1]) lst.Add(new Vector2Int(x, rows - 1));
        }
        for (int y = 0; y < rows; y++)
        {
            if (!used[0, y]) lst.Add(new Vector2Int(0, y));
            if (!used[cols - 1, y]) lst.Add(new Vector2Int(cols - 1, y));
        }

        if (lst.Count == 0) return new Vector2Int(-1, -1);
        return lst[rng.Next(lst.Count)];
    }

    // ============================================================
    // GROW PATH SAFE (CHECK SELF-KILL STEP BY STEP)
    // ============================================================
    private static List<Vector2Int> GrowFromExit_Safe(
        Vector2Int exit,
        bool[,] used,
        int rows, int cols,
        System.Random rng,
        int minLen, int maxLen)
    {
        int length = rng.Next(minLen, maxLen + 1);
        List<Vector2Int> path = new List<Vector2Int>();

        // reverse mục đích: tạo path từ EXIT -> vào trong → đảo lại → START
        path.Add(exit);

        for (int i = 1; i < length; i++)
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
                if (path.Contains(next)) continue;

                cand.Add(next);
            }

            if (cand.Count == 0)
                break;

            Vector2Int chosen = cand[rng.Next(cand.Count)];

            // TRY ADD SAFE
            if (!TryAddNode_Safe(path, chosen, cols))
                break;
        }

        // cuối cùng reverse để thành path START
        path.Reverse();

        return path;
    }

    // ============================================================
    // SAFE APPEND NODE WITH SELF-KILL FIX
    // ============================================================
    private static bool TryAddNode_Safe(List<Vector2Int> path, Vector2Int newNode, int cols)
    {
        path.Add(newNode);

        // convert sang index để check nhanh
        List<int> nodes = new List<int>();
        foreach (var p in path)
            nodes.Add(p.y * cols + p.x);

        if (IsSelfKill(nodes, cols))
        {
            TrimSelfKill(nodes, cols);

            if (nodes.Count < 2)
                return false;

            // convert ngược lại Vector2Int
            path.Clear();
            foreach (var idx in nodes)
            {
                int x = idx % cols;
                int y = idx / cols;
                path.Add(new Vector2Int(x, y));
            }
        }

        return true;
    }

    // ============================================================
    // SELF-KILL CHECK
    // ============================================================
    private static bool IsSelfKill(List<int> list, int cols)
    {
        if (list.Count < 3) return false;

        int head = list[0];
        int neck = list[1];

        int hx = head % cols;
        int hy = head / cols;

        int nx = neck % cols;
        int ny = neck / cols;

        int dx = nx - hx;
        int dy = ny - hy;

        int tx = hx + dx;
        int ty = hy + dy;

        int target = ty * cols + tx;

        for (int i = 2; i < list.Count; i++)
            if (list[i] == target)
                return true;

        return false;
    }

    private static void TrimSelfKill(List<int> list, int cols)
    {
        int head = list[0];
        int neck = list[1];

        int hx = head % cols;
        int hy = head / cols;

        int nx = neck % cols;
        int ny = neck / cols;

        int dx = nx - hx;
        int dy = ny - hy;

        int tx = hx + dx;
        int ty = hy + dy;

        int target = ty * cols + tx;

        int pos = list.IndexOf(target);
        if (pos > 1)
            list.RemoveRange(pos, list.Count - pos);
    }

    // ============================================================
    // REMOVE SNAKES THAT CANNOT ESCAPE
    // ============================================================
    private static void FilterSnakes_CannotEscape(MapInfo map, int rows, int cols)
    {
        List<GeckoDataInMap> keep = new List<GeckoDataInMap>();

        foreach (var g in map.listGeckoInMap)
        {
            if (CanEscape(g, rows, cols))
                keep.Add(g);
        }

        map.listGeckoInMap = keep;
    }

    private static bool CanEscape(GeckoDataInMap g, int rows, int cols)
    {
        int head = g.indexHead;
        int hx = head % cols;
        int hy = head / cols;

        if (hx == 0 || hx == cols - 1 || hy == 0 || hy == rows - 1)
            return true;

        foreach (var d in DIRS)
        {
            int nx = hx + d.x;
            int ny = hy + d.y;

            if (nx < 0 || nx >= cols || ny < 0 || ny >= rows)
                return true;

            int idx = ny * cols + nx;
            if (g.listNode.Contains(idx)) continue;

            return true;
        }

        return false;
    }
}
