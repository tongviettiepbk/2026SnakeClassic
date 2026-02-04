using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Kết quả đánh giá độ khó của 1 map.
/// </summary>
[Serializable]
public class MapDifficultyResult
{
    public float finalScore;       // Điểm cuối cùng (dùng để sort map)

    public float snakeScore;
    public float brickScore;
    public float exitScore;
    public float barrierScore;
    public float stopScore;

    // Thêm thông tin debug
    public int geckoCount;
    public float avgGeckoLength;
    public int maxGeckoLength;
}

/// <summary>
/// Class tính điểm độ khó cho MapInfo.
/// </summary>
public static class MapDifficultyCalculator
{
    // Trọng số tổng thể (có thể tweak tuỳ ý)
    private const float W_SNAKE = 0.55f;
    private const float W_BRICK = 0.25f;
    private const float W_EXIT = 0.10f;
    private const float W_BARRIER = 0.20f;
    private const float W_STOP = 0.05f;

    /// <summary>
    /// Hàm chính: truyền vào MapInfo, trả về kết quả chấm điểm.
    /// </summary>
    public static MapDifficultyResult Compute(MapInfo map)
    {
        if (map == null)
        {
            Debug.LogWarning("MapDifficultyCalculator: MapInfo null.");
            return new MapDifficultyResult();
        }

        int rows = map.row;
        int cols = map.column;
        int totalCells = Mathf.Max(1, rows * cols);

        // --- 1) TÍNH CÁC METRIC GECKO ---
        float snakeScore;
        int geckoCount;
        float avgLength;
        int maxLength;
        ComputeSnakeScore(map, rows, cols, totalCells,
            out snakeScore, out geckoCount, out avgLength, out maxLength);

        // --- 2) BRICK ---
        float brickScore = ComputeBrickScore(map, rows, cols);

        // --- 3) EXIT ---
        float exitScore = ComputeExitScore(map, rows, cols);

        // --- 4) BARRIER ---
        float barrierScore = ComputeBarrierScore(map);

        // --- 5) STOP ---
        float stopScore = ComputeStopScore(map, rows, cols);

        // --- 6) KẾT HỢP TRỌNG SỐ ---
        float finalScore =
            snakeScore * W_SNAKE +
            brickScore * W_BRICK +
            exitScore * W_EXIT +
            barrierScore * W_BARRIER +
            stopScore * W_STOP;

        // Có thể scale lên cho “đẹp số” (vd x1.0 / x1.5 tuỳ cảm nhận)
        // finalScore *= 1.0f;

        return new MapDifficultyResult
        {
            finalScore = finalScore,
            snakeScore = snakeScore,
            brickScore = brickScore,
            exitScore = exitScore,
            barrierScore = barrierScore,
            stopScore = stopScore,
            geckoCount = geckoCount,
            avgGeckoLength = avgLength,
            maxGeckoLength = maxLength
        };
    }

    #region Snake Score

    private static void ComputeSnakeScore(
        MapInfo map,
        int rows,
        int cols,
        int totalCells,
        out float snakeScore,
        out int geckoCount,
        out float avgLength,
        out int maxLength)
    {
        var geckos = map.listGeckoInMap;
        if (geckos == null || geckos.Count == 0)
        {
            snakeScore = 0f;
            geckoCount = 0;
            avgLength = 0;
            maxLength = 0;
            return;
        }

        geckoCount = geckos.Count;
        int totalLength = 0;
        maxLength = 0;

        // Để tính mật độ & entangle, lưu toàn bộ node vào HashSet
        HashSet<int> allSnakeNodes = new HashSet<int>();
        foreach (var g in geckos)
        {
            if (g.listNode == null) continue;
            foreach (var node in g.listNode)
                allSnakeNodes.Add(node);
        }

        // Tính độ dài
        foreach (var g in geckos)
        {
            int len = (g.listNode != null) ? g.listNode.Count : 0;
            totalLength += len;
            if (len > maxLength) maxLength = len;
        }

        avgLength = (geckoCount > 0) ? (float)totalLength / geckoCount : 0f;

        // clusterFactor: mật độ rắn trong map (0 → 10)
        float snakeFillRatio = (float)allSnakeNodes.Count / totalCells; // 0..1
        float clusterFactor = snakeFillRatio * 10f;

        // entangleFactor: mức độ “đan chéo”
        float entangleFactor = ComputeEntangleFactor(allSnakeNodes, rows, cols);

        // loopFactor: số gecko có xu hướng tạo vòng (approx)
        int loopCount = 0;
        foreach (var g in geckos)
        {
            if (IsLoopLikeGecko(g, rows, cols))
                loopCount++;
        }

        // ⭐ TÍNH MAP SIZE FACTOR
        // sqrtCells = độ rộng thực tế của map
        float sqrtCells = Mathf.Sqrt(totalCells); // ví dụ 29x29 → 29
        float mapSizeFactor =
            sqrtCells * 0.6f +
            (totalCells / 100f) * 0.5f;

        // SnakeScore theo công thức đã nói
        snakeScore =
            geckoCount * 1.0f +
            avgLength * 0.5f +
            maxLength * 0.8f +
            clusterFactor * 1.2f +
            entangleFactor * 1.5f +
            loopCount * 2.0f +
            mapSizeFactor; // ⭐ THÊM YẾU TỐ ĐỘ LỚN MAP
    }


    /// <summary>
    /// Tính mức độ entangle: trung bình mỗi node “đụng” bao nhiêu neighbors là rắn.
    /// </summary>
    private static float ComputeEntangleFactor(HashSet<int> allNodes, int rows, int cols)
    {
        if (allNodes == null || allNodes.Count == 0) return 0f;

        int touchCount = 0;
        foreach (int idx in allNodes)
        {
            int x = idx % cols;
            int y = idx / cols;

            // 4-neighbors
            CheckNeighbor(x + 1, y, rows, cols, allNodes, ref touchCount);
            CheckNeighbor(x - 1, y, rows, cols, allNodes, ref touchCount);
            CheckNeighbor(x, y + 1, rows, cols, allNodes, ref touchCount);
            CheckNeighbor(x, y - 1, rows, cols, allNodes, ref touchCount);
        }

        float avgTouch = (float)touchCount / allNodes.Count; // ~0..4
        // Scale lên tí cho score ổn
        return avgTouch * 2f;
    }

    private static void CheckNeighbor(int x, int y, int rows, int cols, HashSet<int> allNodes, ref int touchCount)
    {
        if (x < 0 || x >= cols || y < 0 || y >= rows) return;
        int idx = y * cols + x;
        if (allNodes.Contains(idx)) touchCount++;
    }

    /// <summary>
    /// Heuristic đơn giản: 1 gecko được xem là "loop-like" nếu tồn tại node có >=3 neighbors là rắn.
    /// </summary>
    private static bool IsLoopLikeGecko(GeckoDataInMap gecko, int rows, int cols)
    {
        if (gecko == null || gecko.listNode == null || gecko.listNode.Count < 6)
            return false;

        HashSet<int> nodes = new HashSet<int>(gecko.listNode);

        foreach (int idx in nodes)
        {
            int x = idx % cols;
            int y = idx / cols;

            int neighborCount = 0;

            if (HasNode(nodes, x + 1, y, rows, cols)) neighborCount++;
            if (HasNode(nodes, x - 1, y, rows, cols)) neighborCount++;
            if (HasNode(nodes, x, y + 1, rows, cols)) neighborCount++;
            if (HasNode(nodes, x, y - 1, rows, cols)) neighborCount++;

            if (neighborCount >= 3)
                return true;
        }

        return false;
    }

    private static bool HasNode(HashSet<int> nodes, int x, int y, int rows, int cols)
    {
        if (x < 0 || x >= cols || y < 0 || y >= rows) return false;
        int idx = y * cols + x;
        return nodes.Contains(idx);
    }

    #endregion

    #region Brick Score

    private static float ComputeBrickScore(MapInfo map, int rows, int cols)
    {
        var bricks = map.listBrickInMap;
        var geckos = map.listGeckoInMap;

        if (bricks == null || bricks.Count == 0)
            return 0f;

        int brickCount = bricks.Count;
        int totalHp = 0;
        int maxHp = 0;

        foreach (var b in bricks)
        {
            totalHp += b.hp;
            if (b.hp > maxHp) maxHp = b.hp;
        }

        // Tính khoảng cách brick tới đầu gecko gần nhất
        List<int> headIndices = new List<int>();
        if (geckos != null)
        {
            foreach (var g in geckos)
                headIndices.Add(g.indexHead);
        }

        float avgNearHead = 0f;
        int countDist = 0;

        if (headIndices.Count > 0)
        {
            foreach (var b in bricks)
            {
                int bx = b.indexInMap % cols;
                int by = b.indexInMap / cols;

                int minDist = int.MaxValue;
                foreach (int hIdx in headIndices)
                {
                    int hx = hIdx % cols;
                    int hy = hIdx / cols;
                    int dist = Mathf.Abs(hx - bx) + Mathf.Abs(hy - by); // Manhattan
                    if (dist < minDist) minDist = dist;
                }

                if (minDist != int.MaxValue)
                {
                    avgNearHead += minDist;
                    countDist++;
                }
            }

            if (countDist > 0)
                avgNearHead /= countDist;
        }

        // Brick gần đầu → khó hơn. Ta invert khoảng cách.
        // Giả sử khoảng cách "quan trọng" trong tầm 0..10 ô.
        float distanceFactor = 0f;
        if (countDist > 0)
        {
            float clamped = Mathf.Clamp(10f - avgNearHead, 0f, 10f); // 0..10
            distanceFactor = clamped * 0.1f; // 0..1
        }

        float brickScore =
            brickCount * 1.0f +
            totalHp * 0.05f +
            maxHp * 0.3f +
            distanceFactor;

        return brickScore;
    }

    #endregion

    #region Exit Score

    private static float ComputeExitScore(MapInfo map, int rows, int cols)
    {
        var exits = map.listExitsInMap;
        var geckos = map.listGeckoInMap;

        if (exits == null || exits.Count == 0 || geckos == null || geckos.Count == 0)
            return 0f;

        int exitCount = exits.Count;

        // Gom exit theo color
        Dictionary<int, List<int>> exitsByColor = new Dictionary<int, List<int>>();
        foreach (var e in exits)
        {
            if (!exitsByColor.TryGetValue(e.color, out var list))
            {
                list = new List<int>();
                exitsByColor[e.color] = list;
            }
            list.Add(e.indexInMap);
        }

        int mismatchColor = 0;
        float totalDist = 0f;
        int distCount = 0;

        foreach (var g in geckos)
        {
            int color = g.colorGecko;
            int headIdx = g.indexHead;

            if (!exitsByColor.TryGetValue(color, out var exitsOfColor) || exitsOfColor.Count == 0)
            {
                mismatchColor++;
                continue;
            }

            // Tính khoảng cách head → exit cùng màu gần nhất
            int hx = headIdx % cols;
            int hy = headIdx / cols;

            int minDist = int.MaxValue;
            foreach (int eIdx in exitsOfColor)
            {
                int ex = eIdx % cols;
                int ey = eIdx / cols;
                int dist = Mathf.Abs(ex - hx) + Mathf.Abs(ey - hy);
                if (dist < minDist) minDist = dist;
            }

            if (minDist != int.MaxValue)
            {
                totalDist += minDist;
                distCount++;
            }
        }

        float avgDist = (distCount > 0) ? totalDist / distCount : 0f;

        // ExitScore: nhiều exit + mismatch nhiều + exit xa đầu → khó hơn
        float exitScore =
            exitCount * 0.5f +
            mismatchColor * 2.0f +
            avgDist * 0.1f;

        return exitScore;
    }

    #endregion

    #region Barrier Score

    private static float ComputeBarrierScore(MapInfo map)
    {
        var barriers = map.listBarrierInMap;
        var geckos = map.listGeckoInMap;

        if (barriers == null || barriers.Count == 0 || geckos == null)
            return 0f;

        int rows = map.row;
        int cols = map.column;

        // Convert barrier list to HashSet for fast lookup
        HashSet<int> barrierSet = new HashSet<int>();
        foreach (var b in barriers)
            barrierSet.Add(b.indexInMap);

        int blockedSnakes = 0;

        foreach (var g in geckos)
        {
            if (g.listNode == null || g.listNode.Count < 2)
                continue;

            int head = g.listNode[0];
            int neck = g.listNode[1];

            int headX = head % cols;
            int headY = head / cols;

            int neckX = neck % cols;
            int neckY = neck / cols;

            // Determine direction (dx, dy)
            int dx = headX - neckX;
            int dy = headY - neckY;

            int x = headX;
            int y = headY;

            bool barrierFound = false;

            while (true)
            {
                x += dx;
                y += dy;

                // Out of map
                if (x < 0 || x >= cols || y < 0 || y >= rows)
                    break;

                int idx = y * cols + x;

                // If barrier detected anywhere along the path → rắn này bị block
                if (barrierSet.Contains(idx))
                {
                    blockedSnakes++;
                    barrierFound = true;
                    break;
                }
            }
        }

        // Barrier has the strongest difficulty impact
        float weight = 8;
        float barrierScore = blockedSnakes * weight;

        DebugCustom.ShowDebug("number snack block", blockedSnakes);

        return barrierScore;
    }

    #endregion

    #region Stop Score

    private static float ComputeStopScore(MapInfo map, int rows, int cols)
    {
        var stops = map.listStopInMap;
        var geckos = map.listGeckoInMap;

        if (stops == null || stops.Count == 0)
            return 0f;

        int stopCount = stops.Count;
        int stopNearHead = 0;

        List<int> headIndices = new List<int>();
        if (geckos != null)
        {
            foreach (var g in geckos)
                headIndices.Add(g.indexHead);
        }

        foreach (var s in stops)
        {
            int sx = s.indexInMap % cols;
            int sy = s.indexInMap / cols;

            bool near = false;
            foreach (int hIdx in headIndices)
            {
                int hx = hIdx % cols;
                int hy = hIdx / cols;
                int dist = Mathf.Abs(hx - sx) + Mathf.Abs(hy - sy);
                if (dist <= 2) // trong vòng 2 ô coi là "gần"
                {
                    near = true;
                    break;
                }
            }

            if (near) stopNearHead++;
        }

        float stopScore =
            stopCount * 3.0f +
            stopNearHead * 4.0f;

        return stopScore;
    }

    #endregion

    #region Difficulty Label

    private static string GetDifficultyLabel(float score)
    {
        // Ngưỡng có thể chỉnh sau khi bạn test thực tế
        if (score < 30f) return "Easy";
        if (score < 70f) return "Medium";
        if (score < 120f) return "Hard";
        if (score < 200f) return "VeryHard";
        return "Extreme";
    }

    #endregion
}
