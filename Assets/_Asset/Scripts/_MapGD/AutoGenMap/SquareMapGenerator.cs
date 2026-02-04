using System;
using System.Collections.Generic;
using UnityEngine;

public static class SquareMapGenerator
{
    public static MapInfo GenerateSquareMap(int cols = 26, int rows = 26, int minLen = 2, int maxLen = 30, int? seed = null)
    {
        bool[,] squareMask = BuildSquareMask(cols, rows);
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
            Vector2Int start = FindUnusedCell(squareMask, visited, cols, rows);
            if (start.x < 0)
                break;

            int targetLen = rng.Next(minLen, maxLen + 1);
            List<Vector2Int> path = RandomWalkSnake(start, targetLen, squareMask, visited, cols, rows, rng);

            if (path.Count >= 2)
            {
                foreach (var p in path)
                    visited[p.x, p.y] = true;

                GeckoDataInMap gecko = new GeckoDataInMap();
                gecko.colorGecko = map.listGeckoInMap.Count;

                if(gecko.colorGecko > 5)
                {
                    gecko.colorGecko = rng.Next(0, 6);
                }

                gecko.listNode = new List<int>();

                foreach (var p in path)
                {
                    int index = p.y * cols + p.x;
                    gecko.listNode.Add(index);
                }

                gecko.indexHead = gecko.listNode[0];
                map.listGeckoInMap.Add(gecko);
            }
            else
            {
                visited[start.x, start.y] = true;
            }
        }

        return map;
    }

    private static bool[,] BuildSquareMask(int cols, int rows)
    {
        bool[,] mask = new bool[cols, rows];
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
                mask[x, y] = true;

        return mask;
    }

    private static Vector2Int FindUnusedCell(bool[,] mask, bool[,] visited, int cols, int rows)
    {
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
                if (mask[x, y] && !visited[x, y])
                    return new Vector2Int(x, y);

        return new Vector2Int(-1, -1);
    }

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
                if (!mask[nx, ny])
                    continue;
                if (visited[nx, ny])
                    continue;
                if (path.Contains(new Vector2Int(nx, ny)))
                    continue;

                candidates.Add(new Vector2Int(nx, ny));
            }

            if (candidates.Count == 0)
                break;

            current = candidates[rng.Next(candidates.Count)];
            path.Add(current);
        }

        return path;
    }
}
