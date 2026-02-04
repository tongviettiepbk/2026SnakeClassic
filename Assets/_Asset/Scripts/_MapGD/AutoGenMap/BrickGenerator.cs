using System.Collections.Generic;
using UnityEngine;

public static class BrickGenerator
{
    public static void FillBricks(
        MapInfo map,
        int brickCount,
        int minHP,
        int maxHP,
        int corridorWidth,
        int? seed = null
    )
    {
        System.Random rng = seed.HasValue ? new System.Random(seed.Value) : new System.Random();

        int cols = map.column;
        int rows = map.row;

        bool[,] occupied = new bool[cols, rows];

        // mark snake regions as occupied
        foreach (var g in map.listGeckoInMap)
        {
            foreach (int node in g.listNode)
            {
                int x = node % cols;
                int y = node / cols;
                occupied[x, y] = true;
            }
        }

        for (int i = 0; i < brickCount; i++)
        {
            // random search empty cell
            Vector2Int pos = FindValidEmptyCell(occupied, cols, rows, corridorWidth, rng);
            if (pos.x < 0) break;

            // mark occupied
            occupied[pos.x, pos.y] = true;

            BrickDataInMap brick = new BrickDataInMap();
            brick.indexInMap = pos.y * cols + pos.x;
            brick.hp = rng.Next(minHP, maxHP + 1);

            map.listBrickInMap.Add(brick);
        }
    }

    private static Vector2Int FindValidEmptyCell(
        bool[,] occupied,
        int cols,
        int rows,
        int corridorWidth,
        System.Random rng
    )
    {
        List<Vector2Int> candidates = new List<Vector2Int>();

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                if (occupied[x, y])
                    continue;

                // Avoid snake too close:
                if (IsNearOccupiedSnake(x, y, occupied, cols, rows, corridorWidth))
                    continue;

                candidates.Add(new Vector2Int(x, y));
            }
        }

        if (candidates.Count == 0)
            return new Vector2Int(-1, -1);

        return candidates[rng.Next(candidates.Count)];
    }

    private static bool IsNearOccupiedSnake(int x, int y, bool[,] occ, int cols, int rows, int corridorWidth)
    {
        for (int dy = -corridorWidth; dy <= corridorWidth; dy++)
        {
            for (int dx = -corridorWidth; dx <= corridorWidth; dx++)
            {
                int nx = x + dx;
                int ny = y + dy;
                if (nx < 0 || nx >= cols) continue;
                if (ny < 0 || ny >= rows) continue;

                if (occ[nx, ny])
                    return true;
            }
        }
        return false;
    }
}
