[System.Serializable]
public class BrickDifficultyConfig
{
    public int snakeCount;     // số rắn
    public int brickCount;     // số gạch
    public int minBrickHP;     // HP thấp nhất
    public int maxBrickHP;     // HP cao nhất
    public int corridorWidth;  // độ rộng hành lang rắn đi (độ rối)

    public static BrickDifficultyConfig GetConfig(int difficulty)
    {
        switch (difficulty)
        {
            case 1:
                return new BrickDifficultyConfig
                {
                    snakeCount = 3,
                    brickCount = 5,
                    minBrickHP = 1,
                    maxBrickHP = 1,
                    corridorWidth = 3
                };
            case 2:
                return new BrickDifficultyConfig
                {
                    snakeCount = 4,
                    brickCount = 8,
                    minBrickHP = 1,
                    maxBrickHP = 2,
                    corridorWidth = 2
                };
            case 3:
                return new BrickDifficultyConfig
                {
                    snakeCount = 10,
                    brickCount = 8,
                    minBrickHP = 1,
                    maxBrickHP = 3,
                    corridorWidth = 2
                };
            case 4:
                return new BrickDifficultyConfig
                {
                    snakeCount = 6,
                    brickCount = 20,
                    minBrickHP = 2,
                    maxBrickHP = 4,
                    corridorWidth = 1
                };
            case 5:
                return new BrickDifficultyConfig
                {
                    snakeCount = 8,
                    brickCount = 30,
                    minBrickHP = 3,
                    maxBrickHP = 5,
                    corridorWidth = 1
                };
        }

        return null;
    }

}



public static class MapBrickGenerator
{
    public static MapInfo Generate(
        int cols,
        int rows,
        int difficulty,
        int? seed = null
    )
    {
        var config = BrickDifficultyConfig.GetConfig(difficulty);

        // 1. snake trước
        MapInfo map = SnakeGenerator.GenerateSnakes(
            cols, rows,
            config.snakeCount,
            seed
        );

        // 2. brick sau
        BrickGenerator.FillBricks(
            map,
            config.brickCount,
            config.minBrickHP,
            config.maxBrickHP,
            config.corridorWidth,
            seed
        );

        return map;
    }
}

