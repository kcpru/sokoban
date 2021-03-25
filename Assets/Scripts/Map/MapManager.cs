using UnityEngine;

/// <summary>
/// Class that allows to create a map and provides methods to operate on it.
/// </summary>
public class MapManager : MonoBehaviour
{
    [Header("Map elements")]
    [SerializeField] private GameObject playerElement;
    [SerializeField] private GameObject targetElement;
    [SerializeField] private GameObject groundElement;
    [SerializeField] private GameObject boxElement;

    /// <summary>
    /// Currently load map.
    /// </summary>
    public Map currentMap;

    public GameObject[,] currentElements;

    private Transform MapRoot => transform;
    private const float X_OFFSET = 1f;
    private const float Y_OFFSET = 1f;

    private void Start()
    {
        //ElementType[,] arr = new ElementType[4, 4]
        //{
        //   { ElementType.Player, ElementType.Ground, ElementType.Ground, ElementType.Ground },
        //   { ElementType.Ground, ElementType.Ground, ElementType.Box, ElementType.Ground },
        //   { ElementType.Ground, ElementType.Air, ElementType.Air, ElementType.Ground },
        //   { ElementType.Ground, ElementType.Ground, ElementType.Ground, ElementType.Target }
        //};

        ElementType[,] arr = new ElementType[5, 6]
        {
           { ElementType.Player, ElementType.Ground, ElementType.Ground, ElementType.Ground, ElementType.Ground, ElementType.Target },
           { ElementType.Ground, ElementType.Ground, ElementType.Ground, ElementType.Ground, ElementType.Ground, ElementType.Ground },
           { ElementType.Box, ElementType.Ground, ElementType.Air, ElementType.Air, ElementType.Ground, ElementType.Ground },
           { ElementType.Ground, ElementType.Ground, ElementType.Air, ElementType.Air, ElementType.Ground, ElementType.Ground },
           { ElementType.Ground, ElementType.Ground, ElementType.Box, ElementType.Ground, ElementType.Target, ElementType.Ground }
        };

        Map map = new Map(arr);
        CreateMap(map);
    }

    /// <summary>
    /// Instantiates map elements according to given <seealso cref="Map"/>.
    /// </summary>
    /// <param name="mapToLoad"></param>
    public void CreateMap(Map mapToLoad)
    {
        currentMap = mapToLoad;
        currentElements = new GameObject[currentMap.mapSize.x, currentMap.mapSize.y];
        Vector3 pos = new Vector3(0, 0, 0);

        for(int y = 0; y < currentMap.mapSize.y; y++)
        {
            for (int x = 0; x < currentMap.mapSize.x; x++)
            {
                ElementType elementType = currentMap.mapDefinition[y, x];
                GameObject elementToSpawn = GetMapElement(elementType);

                if (elementToSpawn != null)
                {
                    float yPos = 1f;

                    if (elementType == ElementType.Ground)
                    {
                        yPos = 0f;
                    }
                    else if (elementType == ElementType.Target)
                    {
                        yPos = 0.1f;
                    }

                    if (elementType != ElementType.Ground && elementType != ElementType.Air)
                    {
                        Instantiate(groundElement, new Vector3(pos.x, 0f, -y), Quaternion.identity);
                    }

                    GameObject newElement = 
                        Instantiate(elementToSpawn, new Vector3(pos.x, yPos, -y), Quaternion.identity);
                }

                pos += new Vector3(1, 0, 0);
            }
            pos = new Vector3(0, 0, pos.z);
        }
    }

    /// <summary>
    /// Returns <seealso cref="GameObject"/> that fits to given <seealso cref="ElementType"/>.
    /// </summary>
    /// <param name="type">Type of element to return</param>
    /// <returns></returns>
    public GameObject GetMapElement(ElementType type)
    {
        GameObject element = null;

        switch (type)
        {
            case ElementType.Ground:
                element = groundElement;
                break;
            case ElementType.Player:
                element = playerElement;
                break;
            case ElementType.Air:
                element = null;
                break;
            case ElementType.Box:
                element = boxElement;
                break;
            case ElementType.Target:
                element = targetElement;
                break;
        }

        return element;
    }
}
