using System;
using System.Collections.Generic;
using UnityEngine;

public static class SquareMapGeneratorWithNumberGecko
{
    public static MapInfo GenerateSquareMap(
        int cols = 26,
        int rows = 26,
        int snakeCount = 10,
        int minLen = 2,
        int maxLen = 30,
        int? seed = null)
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

        // ---- sinh đúng số lượng rắn ----
        for (int i = 0; i < snakeCount; i++)
        {
            Vector2Int start = FindUnusedCellRandom(squareMask, visited, cols, rows, rng);

            // không còn ô trống
            if (start.x < 0)
                break;

            int targetLen = rng.Next(minLen, maxLen + 1);
            List<Vector2Int> path = RandomWalkSnake(start, targetLen, squareMask, visited, cols, rows, rng);

            if (path.Count >= 2)
            {
                // đánh dấu visited
                foreach (var p in path)
                    visited[p.x, p.y] = true;

                GeckoDataInMap gecko = new GeckoDataInMap();
                gecko.colorGecko = map.listGeckoInMap.Count;

                // random lại màu nếu > 5
                if (gecko.colorGecko > 5)
                    gecko.colorGecko = rng.Next(0, 6);

                gecko.listNode = new List<int>();

                foreach (var p in path)
                {
                    int index = p.y * cols + p.x;
                    gecko.listNode.Add(index);
                }

                gecko.indexHead = gecko.listNode[0];

                // ============================
                //      KIỂM TRA RẮN HỢP LỆ
                // ============================
                if (!CheckSnakeSelfCollision(cols, rows, gecko))
                {
                    // Rắn tự đâm → không add vào map, bỏ qua luôn
                    DebugCustom.ShowDebugColorRed("Bi dam");

                    continue;
                }

                if (!CheckSnakeHeadToHeadCollision(map.listGeckoInMap, gecko, cols, rows))
                {
                    // Đầu rắn đối đầu → không add vào map, bỏ qua luôn
                    DebugCustom.ShowDebugColorRed("Dau doi dau");
                    continue;
                }

                map.listGeckoInMap.Add(gecko);
            }
            else
            {
                // không tạo được rắn → đánh dấu ô start đã thử
                visited[start.x, start.y] = true;
            }
        }

        return map;
    }

    // ===================================
    // MASK HÌNH VUÔNG
    // ===================================
    private static bool[,] BuildSquareMask(int cols, int rows)
    {
        bool[,] mask = new bool[cols, rows];
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
                mask[x, y] = true;

        return mask;
    }

    // ===================================
    // RANDOM START POINT (FIX CUỘN GÓC)
    // ===================================
    private static Vector2Int FindUnusedCellRandom(bool[,] mask, bool[,] visited, int cols, int rows, System.Random rng)
    {
        List<Vector2Int> candidates = new List<Vector2Int>();

        // gom tất cả cell còn trống
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
                if (mask[x, y] && !visited[x, y])
                    candidates.Add(new Vector2Int(x, y));

        if (candidates.Count == 0)
            return new Vector2Int(-1, -1);

        // random index
        return candidates[rng.Next(candidates.Count)];
    }

    // ===================================
    // RANDOM WALK
    // ===================================
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

        // 4 hướng
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

    private static bool CheckSnakeSelfCollision(int cols, int rows, GeckoDataInMap gecko)
    {
        var nodes = gecko.listNode;

        string data1 = JsonUtility.ToJson(gecko);
        DebugCustom.ShowDebugColor("List Note Gecko", data1);

        // rắn quá ngắn thì không thể tự đâm
        if (nodes == null || nodes.Count < 3)
            return true;

        int head = nodes[0];
        int second = nodes[1];

        // (x, y) head
        int headX = head % cols;
        int headY = head / cols;

        // (x, y) second
        int secondX = second % cols;
        int secondY = second / cols;

        // hướng di chuyển
        int dirX = headX - secondX;
        int dirY = headY - secondY;

        DebugCustom.ShowDebugColor("direciton gecko:" + dirX, dirY);

        // set tối ưu để check nhanh
        HashSet<int> body = new HashSet<int>(nodes);

        // bắt đầu quét từ ô phía trước đầu rắn
        int x = headX + dirX;
        int y = headY + dirY;

        // QUÉT TẤT CẢ CÁC NODE THEO HƯỚNG DIR
        while (x >= 0 && x < cols && y >= 0 && y < rows)
        {
            int idx = y * cols + x;

            // >>> KIỂM TRA HÀNG XÓM CÓ NẰM TRONG VỊ TRÍ RẮN KHÔNG <<<
            if (body.Contains(idx))
            {
                return false; // tự đâm mình
            }

            x += dirX;
            y += dirY;
        }

        return true; // không đâm
    }

    private static Vector2Int GetSnakeDirection(GeckoDataInMap snake, int cols)
    {
        if (snake.listNode.Count < 2)
            return Vector2Int.zero;

        int head = snake.listNode[0];
        int second = snake.listNode[1];

        int hx = head % cols;
        int hy = head / cols;

        int sx = second % cols;
        int sy = second / cols;

        return new Vector2Int(hx -sx, hy-sy); // hướng head -> second
    }

    private static bool IsOppositeDir(Vector2Int a, Vector2Int b)
    {
        return (a.x == -b.x && a.y == b.y) ||
               (a.y == -b.y && a.x == b.x);
    }

    public static bool CheckSnakeHeadToHeadCollision(
    List<GeckoDataInMap> allSnakes,
    GeckoDataInMap newSnake,
    int cols,
    int rows)
    {
        if (newSnake.listNode.Count < 2)
            return true;

        // Lấy direction con mới
        Vector2Int newDir = GetSnakeDirection(newSnake, cols);

        int newHead = newSnake.listNode[0];
        int newHX = newHead % cols;
        int newHY = newHead / cols;

        foreach (var snake in allSnakes)
        {
            if (snake.listNode.Count < 2)
                continue;

            int head = snake.listNode[0];
            int hx = head % cols;
            int hy = head / cols;

            // 1. Check cùng hàng
            if (newHY == hy)
            {
                // 2. Check đang nhìn nhau theo chiều ngang
                Vector2Int dirOther = GetSnakeDirection(snake, cols);

                if (IsOppositeDir(newDir, dirOther))
                {
                    // đầu thẳng hàng AND hướng đối nghịch
                    return false;
                }
            }

            // 1. Check cùng cột
            if (newHX == hx)
            {
                // 2. Check đang nhìn nhau theo chiều dọc
                Vector2Int dirOther = GetSnakeDirection(snake, cols);

                if (IsOppositeDir(newDir, dirOther))
                {
                    return false;
                }
            }
        }

        return true; // không có đối đầu
    }
}
