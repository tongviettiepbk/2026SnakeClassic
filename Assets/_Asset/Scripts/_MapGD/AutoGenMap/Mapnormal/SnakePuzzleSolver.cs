using System.Collections.Generic;

public static class SnakePuzzleSolver
{
    public static bool IsSolvable(Dictionary<int, List<int>> dep, int snakeCount)
    {
        HashSet<int> solved = new HashSet<int>();

        for (int loop = 0; loop < snakeCount; loop++)
        {
            bool progress = false;

            for (int s = 0; s < snakeCount; s++)
            {
                if (solved.Contains(s)) continue;

                bool canSolve = true;

                foreach (int b in dep[s])
                    if (!solved.Contains(b))
                        canSolve = false;

                if (canSolve)
                {
                    solved.Add(s);
                    progress = true;
                    break;
                }
            }

            if (!progress)
                return false;
        }

        return solved.Count == snakeCount;
    }
}
