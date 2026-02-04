using System;
using System.Collections.Generic;
using UnityEngine;

public static class ShapeMapGenerator
{
    public enum ShapeType
    {
        Heart,
        Circle,
        Triangle,
        Star,
        Tree
    }

    public static MapInfo Generate(int cols, int rows, ShapeType shape, int minLen = 2, int maxLen = 30, int? seed = null)
    {
        bool[,] mask = BuildMask(cols, rows, shape);
        return SnakeRandomFill(mask, cols, rows, minLen, maxLen, seed);
    }

    // -------------------------
    // MASK CHO CÁC HÌNH
    // -------------------------
    private static bool[,] BuildMask(int cols, int rows, ShapeType shape)
    {
        switch (shape)
        {
            case ShapeType.Heart:
                return BuildHeart(cols, rows);

            case ShapeType.Circle:
                return BuildCircle(cols, rows);

            case ShapeType.Triangle:
                return BuildTriangle(cols, rows);

            case ShapeType.Star:
                return BuildStar(cols, rows);

            case ShapeType.Tree:
                return BuildTree(cols, rows);

            default:
                return BuildCircle(cols, rows);
        }
    }

    // SHAPE 1: HEART
    private static bool[,] BuildHeart(int cols, int rows)
    {
        bool[,] m = new bool[cols, rows];
        float sx = 2.4f / (cols - 1);
        float sy = 2.4f / (rows - 1);

        for (int y = 0; y < rows; y++)
        {
            float fy = 1.2f - y * sy;
            for (int x = 0; x < cols; x++)
            {
                float fx = -1.2f + x * sx;
                float a = fx * fx + fy * fy - 1f;
                float v = a * a * a - fx * fx * (fy * fy * fy);
                m[x, y] = v <= 0f;
            }
        }
        return m;
    }

    // SHAPE 2: CIRCLE
    private static bool[,] BuildCircle(int cols, int rows)
    {
        bool[,] m = new bool[cols, rows];
        float rx = cols / 2f;
        float ry = rows / 2f;
        float R = Mathf.Min(rx, ry) * 0.9f;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                float dx = x - rx;
                float dy = y - ry;
                m[x, y] = (dx * dx + dy * dy <= R * R);
            }
        }
        return m;
    }

    // SHAPE 3: TRIANGLE
    private static bool[,] BuildTriangle(int cols, int rows)
    {
        bool[,] m = new bool[cols, rows];

        for (int y = 0; y < rows; y++)
        {
            int xMin = (cols / 2) - (y / 2);
            int xMax = (cols / 2) + (y / 2);
            for (int x = 0; x < cols; x++)
            {
                if (x >= xMin && x <= xMax)
                    m[x, y] = true;
            }
        }
        return m;
    }

    // SHAPE 4: STAR – đơn giản hóa bằng 5 tam giác
    private static bool[,] BuildStar(int cols, int rows)
    {
        bool[,] m = new bool[cols, rows];
        float cx = cols / 2f;
        float cy = rows / 2f;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                // bản star đơn giản hoá làm 5 cánh
                float dx = Mathf.Abs(x - cx);
                float dy = Mathf.Abs(y - cy);

                if (dy < (rows * 0.15f) || dx < (cols * 0.15f))
                    m[x, y] = true;
            }
        }
        return m;
    }

    // SHAPE 5: TREE – giống hình bạn vừa gửi
    private static bool[,] BuildTree(int cols, int rows)
    {
        bool[,] m = new bool[cols, rows];

        for (int y = 0; y < rows; y++)
        {
            int w = rows - y;             // càng xuống càng rộng
            int xMin = (cols / 2) - (w / 2);
            int xMax = (cols / 2) + (w / 2);

            for (int x = 0; x < cols; x++)
            {
                if (x >= xMin && x <= xMax)
                    m[x, y] = true;
            }
        }

        return m;
    }

    // -------------------------
    // GENERATOR SNAKE
    // -------------------------
    private static MapInfo SnakeRandomFill(bool[,] mask, int cols, int rows, int minLen, int maxLen, int? seed)
    {
        bool[,] used = new bool[cols, rows];
        System.Random rng = seed.HasValue ? new System.Random(seed.Value) : new System.Random();

        MapInfo map = new MapInfo();
        map.column = cols;
        map.row = rows;
        map.listGeckoInMap = new List<GeckoDataInMap>();

        while (true)
        {
            Vector2Int s = FindStart(mask, used, cols, rows);
            if (s.x < 0)
                break;

            int len = rng.Next(minLen, maxLen + 1);
            List<Vector2Int> path = RandomWalk(mask, used, cols, rows, len, s, rng);

            if (path.Count >= 2)
            {
                GeckoDataInMap gecko = new GeckoDataInMap();
                gecko.colorGecko = map.listGeckoInMap.Count;
                gecko.listNode = new List<int>();

                foreach (var p in path)
                {
                    used[p.x, p.y] = true;
                    int index = p.y * cols + p.x;
                    gecko.listNode.Add(index);
                }

                gecko.indexHead = gecko.listNode[0];
                map.listGeckoInMap.Add(gecko);
            }
            else
            {
                used[s.x, s.y] = true;
            }
        }

        return map;
    }

    private static Vector2Int FindStart(bool[,] mask, bool[,] used, int cols, int rows)
    {
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
                if (mask[x, y] && !used[x, y])
                    return new Vector2Int(x, y);

        return new Vector2Int(-1, -1);
    }

    private static readonly Vector2Int[] DIRS =
    {
        new Vector2Int(1,0),
        new Vector2Int(-1,0),
        new Vector2Int(0,1),
        new Vector2Int(0,-1)
    };

    private static List<Vector2Int> RandomWalk(bool[,] mask, bool[,] used, int cols, int rows, int target, Vector2Int start, System.Random rng)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        path.Add(start);

        Vector2Int cur = start;

        for (int i = 1; i < target; i++)
        {
            List<Vector2Int> cand = new List<Vector2Int>();

            foreach (var d in DIRS)
            {
                int nx = cur.x + d.x;
                int ny = cur.y + d.y;
                if (nx < 0 || nx >= cols || ny < 0 || ny >= rows) continue;
                if (!mask[nx, ny]) continue;
                if (used[nx, ny]) continue;
                if (path.Contains(new Vector2Int(nx, ny))) continue;

                cand.Add(new Vector2Int(nx, ny));
            }

            if (cand.Count == 0)
                break;

            cur = cand[rng.Next(cand.Count)];
            path.Add(cur);
        }

        return path;
    }
}
