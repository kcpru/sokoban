using UnityEngine;
using System.Collections;

/// <summary>
/// Class that allows to create a map and provides methods to operate on it.
/// </summary>
public class MapManager : MonoBehaviour
{
    [Header("Movement settings")]
    [SerializeField] private float moveSpeed = 10f;

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
    public GameObject Player { get; private set; }

    public Vector2Int PlayerPosition
    {
        get
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                for (int x = 0; x < currentMap.mapSize.x; x++)
                {
                    if (currentMap.mapDefinition[y, x] == ElementType.Player)
                        return new Vector2Int(x, y);
                }
            }

            return Vector2Int.zero;
        }
    }

    private Transform MapRoot => transform;
    private const float X_OFFSET = 1f;
    private const float Y_OFFSET = 1f;

    public static MapManager CurrentMapManager { get; private set; }

    private void Awake() => CurrentMapManager = this;

    private void Start()
    {
        //ElementType[,] arr = new ElementType[4, 4]
        //{
        //   { ElementType.Player, ElementType.Ground, ElementType.Ground, ElementType.Ground },
        //   { ElementType.Ground, ElementType.Ground, ElementType.Box, ElementType.Ground },
        //   { ElementType.Ground, ElementType.Air, ElementType.Air, ElementType.Ground },
        //   { ElementType.Ground, ElementType.Ground, ElementType.Ground, ElementType.Target }
        //};

        ElementType[,] arr = new ElementType[6, 7]
        {
           { ElementType.Ground, ElementType.Player, ElementType.Ground, ElementType.Ground, ElementType.Ground, ElementType.Ground, ElementType.Target },
           { ElementType.Ground, ElementType.Ground, ElementType.Ground, ElementType.Ground, ElementType.Ground, ElementType.Ground, ElementType.Ground },
           { ElementType.Ground, ElementType.Box, ElementType.Ground, ElementType.Ground, ElementType.Ground, ElementType.Ground, ElementType.Ground },
           { ElementType.Ground, ElementType.Ground, ElementType.Ground, ElementType.Ground, ElementType.Ground, ElementType.Ground, ElementType.Ground },
           { ElementType.Ground, ElementType.Ground, ElementType.Ground, ElementType.Box, ElementType.Ground, ElementType.Target, ElementType.Ground },
           { ElementType.Ground, ElementType.Ground, ElementType.Ground, ElementType.Ground, ElementType.Ground, ElementType.Ground, ElementType.Ground }
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
        currentElements = new GameObject[currentMap.mapSize.y, currentMap.mapSize.x];
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
                        Instantiate(groundElement, new Vector3(pos.x, 0f, y), Quaternion.identity, MapRoot);
                    }

                    GameObject newElem = Instantiate(elementToSpawn, new Vector3(pos.x, yPos, y), Quaternion.identity, MapRoot);

                    if (elementType != ElementType.Ground && elementType != ElementType.Air)
                        currentElements[y, x] = newElem;
                    else
                        currentElements[y, x] = null;

                    if (elementType == ElementType.Player)
                        Player = currentElements[y, x];
                }

                pos += new Vector3(1, 0, 0);
            }
            pos = new Vector3(0, 0, pos.z);
        }
    }

    /// <summary>
    /// Moves player to new position. Returns true if player has been moved correctly.
    /// </summary>
    public bool MovePlayer(Vector2Int newPos, Vector2Int dir)
    {
        Vector2Int oldPlayerPos = PlayerPosition;

        if (newPos.x >= currentMap.mapSize.x || newPos.y >= currentMap.mapSize.y || newPos.x < 0 || newPos.y < 0)
            return false;

        if (GetElementType(newPos) == ElementType.Air)
            return false;

        if (GetElementType(newPos) == ElementType.Box || GetElementType(newPos) == ElementType.DoneTarget)
        {
            if (GetElementType(newPos + dir) == ElementType.Ground || GetElementType(newPos + dir) == ElementType.Target)
            {
                Vector2Int newBoxPos = new Vector2Int(newPos.x + dir.x, newPos.y + dir.y);
                GameObject box = currentElements[newPos.y, newPos.x];

                print($"old player pos = {oldPlayerPos}");
                print($"new pos = {newPos}");
                print($"new box pos = {newBoxPos}");

                Player.transform.position = new Vector3(newPos.x, 1f, newPos.y);
                currentElements[newPos.y, newPos.x].transform.position = new Vector3(newBoxPos.x, 1f, newBoxPos.y);

                //StartCoroutine(SmoothMoveElement(Player.transform, new Vector3(newPos.x, 1f, newPos.y)));
                //StartCoroutine(SmoothMoveElement(currentElements[newPos.y, newPos.x].transform, new Vector3(newBoxPos.x, 1f, newBoxPos.y)));

                if (GetElementType(newBoxPos) == ElementType.Target)
                {
                    Debug.Log("<color=green>ENTER TARGET</color>");
                }

                if(GetElementType(newPos) == ElementType.DoneTarget)
                {
                    Debug.Log("<color=red>EXIT TARGET</color>");
                }

                currentMap.mapDefinition[oldPlayerPos.y, oldPlayerPos.x] = ElementType.Ground;
                currentMap.mapDefinition[newPos.y, newPos.x] = ElementType.Player;
                currentMap.mapDefinition[newBoxPos.y, newBoxPos.x] =
                    currentMap.mapDefinition[newBoxPos.y, newBoxPos.x] == ElementType.Target ? ElementType.DoneTarget : ElementType.Box;

                currentElements[oldPlayerPos.y, oldPlayerPos.x] = null;
                currentElements[newPos.y, newPos.x] = Player;
                currentElements[newBoxPos.y, newBoxPos.x] = box;
                print("move pb");
                PrintArrays();
                return true;
            }
            else
            {
                return false;
            }
        }

        print($"old player pos = {oldPlayerPos}");
        print($"new pos = {newPos}");

        Player.transform.position = new Vector3(newPos.x, 1f, newPos.y);
        //StartCoroutine(SmoothMoveElement(Player.transform, new Vector3(newPos.x, 1f, newPos.y)));

        currentMap.mapDefinition[oldPlayerPos.y, oldPlayerPos.x] = ElementType.Ground;
        currentMap.mapDefinition[newPos.y, newPos.x] = ElementType.Player;

        currentElements[oldPlayerPos.y, oldPlayerPos.x] = null;
        currentElements[newPos.y, newPos.x] = Player;
        print("move p");
        PrintArrays();

        return true;
    }

    public void PrintArrays()
    {
        for(int y = 0; y < currentMap.mapSize.y; y++)
        {
            string s = "";

            for(int x = 0; x < currentMap.mapSize.x; x++)
            {
                s += currentMap.mapDefinition[y, x] + ", ";
            }

            Debug.Log(s);
        }

        for (int y = 0; y < currentMap.mapSize.y; y++)
        {
            string s = "";

            for (int x = 0; x < currentMap.mapSize.x; x++)
            {
                s += (currentElements[y, x] == null ? "null" : currentElements[y, x].name) + ", ";
            }

            Debug.Log(s);
        }
    }

    /// <summary>
    /// Smoothly moves given element to given position.
    /// </summary>
    private IEnumerator SmoothMoveElement(Transform element, Vector3 newPos)
    {
        while (element.position != newPos) 
        {
            Vector3 pos = Vector3.Lerp(element.transform.position, newPos, Time.deltaTime * moveSpeed);
            element.transform.position = pos;
            yield return null;
        }

        element.position = new Vector3(Mathf.RoundToInt(element.position.x), element.position.y, Mathf.RoundToInt(element.position.z));
    }

    /// <summary>
    /// Returns type of map element at given position.
    /// </summary>
    public ElementType GetElementType(Vector2Int pos)
    {
        if (pos.x >= currentMap.mapSize.x || pos.y >= currentMap.mapSize.y || pos.x < 0 || pos.y < 0)
            return ElementType.Air;

        return currentMap.mapDefinition[pos.y, pos.x];
    }

    /// <summary>
    /// Returns type of map element at given position.
    /// </summary>
    public ElementType GetElementType(int x, int y) => GetElementType(new Vector2Int(x, y));

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
