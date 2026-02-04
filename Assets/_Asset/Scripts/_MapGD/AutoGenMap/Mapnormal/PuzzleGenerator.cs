using System.Collections.Generic;
using UnityEngine;

public static class PuzzleGenerator
{
    public static MapInfo Generate(
        int cols,
        int rows,
        int difficulty,
        int snakeRequested,
        int? seed = null
    )
    {
        System.Random rng = seed.HasValue ? new System.Random(seed.Value) : new System.Random();

        int maxLoop = 5000;
        int loop = 0;

        while (true)
        {
            loop++;
            if (loop > maxLoop)
            {
                Debug.LogWarning("PuzzleGenerator: MAX LOOP REACHED → trả map đơn giản nhất hiện có.");
                // fallback: retry thêm 1 map bất kỳ
                return SnakeGenerator.GenerateSnakes(cols, rows, snakeRequested, rng.Next());
            }

            int localSeed = rng.Next();

            MapInfo map = SnakeGenerator.GenerateSnakes(
                cols, rows,
                snakeRequested,
                localSeed
            );

            int realCount = map.listGeckoInMap.Count;
            if (realCount == 0)
            {
                Debug.Log("Fail: không sinh được rắn nào → thử lại");
                continue;
            }

            var dep = PuzzleSnakeAnalyzer.BuildDependency(map);
            int depth = MeasureDifficulty(dep);

            bool solvable = SnakePuzzleSolver.IsSolvable(dep, realCount);

            bool allSelf = true;
            foreach (var g in map.listGeckoInMap)
            {
                if (!PuzzleSnakeAnalyzer.SnakeCanEscapeSelf(g, cols, rows))
                {
                    allSelf = false;
                    break;
                }
            }

            // Debug output
            if (!solvable)
                Debug.Log($"FAIL [{localSeed}] - UNSOLVABLE");

            if (!allSelf)
                Debug.Log($"FAIL [{localSeed}] - SELF BLOCK");

            if (!MatchDifficulty(depth, difficulty))
                Debug.Log($"FAIL [{localSeed}] - depth={depth} không phù hợp diff={difficulty}");

            if (solvable && MatchDifficulty(depth, difficulty) && allSelf)
            {
                Debug.Log($"OK! seed={localSeed}, realSnakes={realCount}, depth={depth}, diff={difficulty}");
                return map;
            }
        }
    }

    // ================================================================
    // MEASURE PUZZLE DEPENDENCY DEPTH
    // ================================================================

    private static int MeasureDifficulty(Dictionary<int, List<int>> dep)
    {
        int maxDepth = 0;

        foreach (var kv in dep)
        {
            int d = DFS(kv.Key, dep, new HashSet<int>());
            if (d > maxDepth) maxDepth = d;
        }

        return maxDepth;
    }

    private static int DFS(int g, Dictionary<int, List<int>> dep, HashSet<int> visited)
    {
        if (visited.Contains(g))
            return 0;

        visited.Add(g);

        int best = 0;
        foreach (int b in dep[g])
            best = Mathf.Max(best, 1 + DFS(b, dep, visited));

        return best;
    }

    // ================================================================
    // DIFFICULTY RANGES — ĐÃ LÀM MỀM HƠN (KHÔNG BỊ VÒNG LẶP)
    // ================================================================

    private static bool MatchDifficulty(int depth, int diff)
    {
        return diff switch
        {
            0 => depth <= 1,            // dễ
            1 => depth >= 1 && depth <= 3,   // trung bình
            2 => depth >= 2 && depth <= 4,   // khó
            3 => depth >= 3,                 // rất khó
            _ => true
        };
    }
}
