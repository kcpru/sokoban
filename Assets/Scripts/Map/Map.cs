using UnityEngine;

/// <summary>
/// Class that defines a basic sokoban map and provides methods to operate on it.
/// </summary>
public class Map
{
    public string name;
    public Vector2Int mapSize;
    public ElementType[,] mapDefinition;
    public Biomes biomeType = Biomes.Grass;
    public Difficulty difficulty = Difficulty.Easy;

    /// <summary>
    /// Checks whether the map is defined and playable.
    /// </summary>
    public bool IsMapDefined => mapDefinition != null;

    public const int MIN_X = 4;
    public const int MAX_X = 30;

    public const int MIN_Y = 4;
    public const int MAX_Y = 20;

    /// <summary>
    /// Returns true if all goals are done, means they have boxes on themselves.
    /// </summary>
    public bool IsAllGoalsDone
    {
        get
        {
            int boxesCount = 0;

            for (int i = 0; i < mapDefinition.GetLength(0); i++)
            {
                for (int j = 0; j < mapDefinition.GetLength(1); j++)
                {
                    if (mapDefinition[i, j] == ElementType.Box)
                        boxesCount++;
                }
            }

            if (boxesCount == 0)
                return true;
            else
                return false;
        }
    }

    /// <summary>
    /// Creates new map which contains given elements.
    /// </summary>
    /// <param name="map">Elements</param>
    public Map(string name, ElementType[,] map)
    {
        mapSize = new Vector2Int(map.GetLength(1), map.GetLength(0));

        if (!IsOnePlayer(ref map))
        {
            Debug.LogError("Map has to contain only one player element!");
            return;
        }

        if(!ValidateBoxes(ref map))
        {
            Debug.LogError("Count of boxes has to equal count of targets and must at least one");
            return;
        }

        this.name = name;
        mapDefinition = (ElementType[,])map.Clone();
    }

    /// <summary>
    /// Creates new map which contains given elements.
    /// </summary>
    /// <param name="map">Elements</param>
    /// <param name="biomeType">Type of biome</param>
    /// <param name="difficulty">Level of difficulty</param>
    public Map(string name, ElementType[,] map, Biomes biomeType, Difficulty difficulty)
    {
        mapSize = new Vector2Int(map.GetLength(1), map.GetLength(0));

        if (!IsOnePlayer(ref map))
        {
            Debug.LogError("Map has to contain only one player element!");
            return;
        }

        if (!ValidateBoxes(ref map))
        {
            Debug.LogError("Count of boxes has to equal count of targets and must at least one!");
            return;
        }

        this.name = name;
        this.difficulty = difficulty;
        this.biomeType = biomeType;
        mapDefinition = (ElementType[,])map.Clone();
    }

    /// <summary>
    /// Return count of given element on the map.
    /// </summary>
    public int ElementCount(ElementType elementType)
    {
        int count = 0;

        for (int i = 0; i < mapDefinition.GetLength(0); i++)
        {
            for (int j = 0; j < mapDefinition.GetLength(1); j++)
            {
                if (mapDefinition[i, j] == elementType)
                    count++;
            }
        }

        return count;
    }

    /// <summary>
    /// Returns cloned map.
    /// </summary>
    /// <returns></returns>
    public Map Clone()
    {
        ElementType[,] elements = (ElementType[,])mapDefinition.Clone();
        Map map = new Map(name, elements, biomeType, difficulty);
        return map;
    }

    /// <summary>
    /// Checks whether the given map has only one player.
    /// </summary>
    /// <param name="map">Array to check.</param>
    public static bool IsOnePlayer(ref ElementType[,] map)
    {
        bool hasPlayer = false;

        for (int i = 0; i < map.GetLength(0); i++)
        {
            for (int j = 0; j < map.GetLength(1); j++)
            {
                if (hasPlayer && (map[i, j] == ElementType.Player || map[i, j] == ElementType.PlayerOnTarget))
                    return false;

                if (!hasPlayer && (map[i, j] == ElementType.Player || map[i, j] == ElementType.PlayerOnTarget))
                    hasPlayer = true;
            }
        }

        return true;
    }

    /// <summary>
    /// Checks whether count of boxes equals count of targets.
    /// </summary>
    public static bool ValidateBoxes(ref ElementType[,] map)
    {
        int boxesCount = 0, targetsCount = 0;

        for(int i = 0; i < map.GetLength(0); i++)
        {
            for(int j = 0; j < map.GetLength(1); j++)
            {
                if (map[i, j] == ElementType.Box)
                    boxesCount++;

                if (map[i, j] == ElementType.Target || map[i, j] == ElementType.PlayerOnTarget)
                    targetsCount++;
            }
        }

        if (boxesCount != targetsCount)
        {
            return false;
        }
        else
        {
            return boxesCount > 0 ? true : false;
        }
    }

}
