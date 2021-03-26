using UnityEngine;

/// <summary>
/// Class that defines a basic sokoban map and provides methods to operate on it.
/// </summary>
public class Map
{
    public Vector2Int mapSize;
    public ElementType[,] mapDefinition;
    public Biomes biomeType;
    public LevelOfDifficulty levelOfDifficulty;

    /// <summary>
    /// Checks whether the map is defined and playable.
    /// </summary>
    public bool IsMapDefined => mapDefinition != null;

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
    public Map(ElementType[,] map)
    {
        mapSize = new Vector2Int(map.GetLength(1), map.GetLength(0));

        if (!IsOnePlayer(ref map))
        {
            Debug.LogError("Map has to contain only one player element!");
            return;
        }

        if(!ValidateBoxes(ref map))
        {
            Debug.LogError("Count of boxes has to equal count of targets!");
            return;
        }

        mapDefinition = map;
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

        if (boxesCount == targetsCount)
            return true;
        else
            return false;
    }
}
