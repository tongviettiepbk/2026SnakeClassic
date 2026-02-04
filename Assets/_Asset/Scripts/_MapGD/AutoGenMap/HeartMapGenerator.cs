using System;
using System.Collections.Generic;
using UnityEngine;

public static class HeartMapGenerator
{
    public static MapInfo GenerateHeartMap(int cols = 26, int rows = 26, int minLen = 2, int maxLen = 30, int? seed = null)
    {
        bool[,] heartMask = BuildHeartMask(cols, rows);
        bool[,] visited = new bool[cols, rows];

        MapInfo map = new MapInfo();
        map.column = cols;
        map.row = rows;
        map.listGeckoInMap = new List<GeckoDataInMap>();
        map.listBrickInMap = new List<BrickDataInMap>();
        map.listStopInMap = new List<StopDataInMap>();
        map.listExitsInMap = new List<ExitHoleDataInMap>();
        map.listBarrierInMap = new List<BarrierDataInMap>();

        System.Random rng = seed.HasValue ? new System.Random(seed.Value) : new System.Random();

        while (true)
        {
            Vector2Int start = FindUnusedHeartCell(heartMask, visited, cols, rows);
            if (start.x < 0)
                break; // không còn ô trong mask

            int targetLen = rng.Next(minLen, maxLen + 1);
            List<Vector2Int> path = RandomWalkSnake(start, targetLen, heartMask, visited, cols, rows, rng);

            if (path.Count >= 2)
            {
                // đánh dấu visited
                foreach (var p in path)
                {
                    visited[p.x, p.y] = true;
                }

                // convert sang GeckoDataInMap
                GeckoDataInMap gecko = new GeckoDataInMap();
                gecko.colorGecko = map.listGeckoInMap.Count; // tạm dùng index làm màu
                gecko.listNode = new List<int>();

                foreach (var p in path)
                {
                    int index = p.y * cols + p.x;   // giống MapGrid2dHelper
                    gecko.listNode.Add(index);
                }

                gecko.indexHead = gecko.listNode[0]; // phần tử đầu là head
                map.listGeckoInMap.Add(gecko);
            }
            else
            {
                // path quá ngắn (< 2), bỏ, đánh dấu đã thử
                visited[start.x, start.y] = true;
            }
        }

        return map;
    }

    // ============================
    // Heart mask 26x26
    // ============================
    private static bool[,] BuildHeartMask(int cols, int rows)
    {
        bool[,] mask = new bool[cols, rows];

        float sx = 2.4f / (cols - 1);
        float sy = 2.4f / (rows - 1);

        for (int y = 0; y < rows; y++)
        {
            float fy = 1.2f - y * sy;
            for (int x = 0; x < cols; x++)
            {
                float fx = -1.2f + x * sx;
                float a = fx * fx + fy * fy - 1f;
                float v = a * a * a - fx * fx * fy * fy * fy;

                mask[x, y] = v <= 0f; // true nếu nằm trong hình trái tim
            }
        }

        return mask;
    }

    // tìm 1 ô trong heartMask chưa được dùng
    private static Vector2Int FindUnusedHeartCell(bool[,] mask, bool[,] visited, int cols, int rows)
    {
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                if (mask[x, y] && !visited[x, y])
                {
                    return new Vector2Int(x, y);
                }
            }
        }

        return new Vector2Int(-1, -1);
    }

    // random walk tạo 1 con rắn
    private static List<Vector2Int> RandomWalkSnake(
        Vector2Int start,
        int targetLen,
        bool[,] mask,
        bool[,] visited,
        int cols,
        int rows,
        System.Random rng)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        path.Add(start);

        Vector2Int current = start;

        Vector2Int[] dirs = new Vector2Int[]
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0,-1)
        };

        for (int i = 1; i < targetLen; i++)
        {
            List<Vector2Int> candidates = new List<Vector2Int>();

            foreach (var d in dirs)
            {
                int nx = current.x + d.x;
                int ny = current.y + d.y;

                if (nx < 0 || nx >= cols || ny < 0 || ny >= rows)
                    continue;

                if (!mask[nx, ny]) continue;             // ngoài hình trái tim
                if (visited[nx, ny]) continue;           // đã thuộc snake khác
                if (path.Contains(new Vector2Int(nx, ny))) continue; // tránh tự cắn đuôi

                candidates.Add(new Vector2Int(nx, ny));
            }

            if (candidates.Count == 0)
                break; // hết đường đi

            current = candidates[rng.Next(candidates.Count)];
            path.Add(current);
        }

        return path;
    }
}
