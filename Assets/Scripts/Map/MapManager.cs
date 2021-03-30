using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// Class that allows to create a map and provides methods to operate on it.
/// </summary>
public class MapManager : MonoBehaviour
{
    [Header("Movement settings")]
    [SerializeField] private float moveSpeed = 7f;

    [Header("Map creation settings")]
    [SerializeField] private bool skipCreateAnimation = false;
    [SerializeField] private float createElementDelay = 0.1f;
                     public float destroyElementDelay = 0.05f;
    [SerializeField] private float createAnimationScale = 1.2f;
    [SerializeField] private float createAnimationUpScaleSpeed = 8f;
    [SerializeField] private float createAnimationDownScaleSpeed = 5f;

    /// <summary>
    /// Currently loaded map.
    /// </summary>
    public Map currentMap;

    public GameObject[,] currentElements;
    [HideInInspector] public List<GameObject> allCreatedElements = new List<GameObject>();
    public GameObject Player { get; private set; }

    /// <summary>
    /// Current position of the player on the map.
    /// </summary>
    public Vector2Int PlayerPosition
    {
        get
        {
            if(currentMap == null) return Vector2Int.zero;

            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                for (int x = 0; x < currentMap.mapSize.x; x++)
                {
                    if (currentMap.mapDefinition[y, x] == ElementType.Player || currentMap.mapDefinition[y, x] == ElementType.PlayerOnTarget)
                        return new Vector2Int(x, y);
                }
            }

            return Vector2Int.zero;
        }
    }

    private const float X_OFFSET = 1f;
    private const float Y_OFFSET = 1f;

    private MapDecoration mapDecorator;

    public static MapManager CurrentMapManager { get; private set; }
    public bool CanMove { get; set; } = false;
    public Transform MapRoot => transform;

    /// <summary>
    /// Raises when player moves.
    /// </summary>
    public event Action<Vector2Int> OnPlayerMove;

    private void Awake() => CurrentMapManager = this;

    private void Start() => mapDecorator = GetComponent<MapDecoration>();

    /// <summary>
    /// Instantiates map elements according to given <seealso cref="Map"/>.
    /// </summary>
    /// <param name="mapToLoad"></param>
    public void CreateMap(Map mapToLoad) => StartCoroutine(CreateMapCoroutine(mapToLoad));

    /// <summary>
    /// Clears currently spawned map.
    /// </summary>
    public void ClearMap(bool removeDecorations) => StartCoroutine(ClearMapCoroutine(removeDecorations));

    /// <summary>
    /// Clears currently spawned map and removes all decorations around map.
    /// </summary>
    public void ClearMap() => StartCoroutine(ClearMapCoroutine(true));

    private IEnumerator CreateMapCoroutine(Map mapToLoad)
    {
        currentMap = mapToLoad;
        currentElements = new GameObject[currentMap.mapSize.y, currentMap.mapSize.x];
        Vector3 pos = new Vector3(0, 0, 0);

        for (int y = 0; y < currentMap.mapSize.y; y++)
        {
            for (int x = 0; x < currentMap.mapSize.x; x++)
            {
                ElementType elementType = currentMap.mapDefinition[y, x];
                GameObject elementToSpawn = MapElementsManager.CurrentManager.GetMapElement(elementType, currentMap.biomeType);

                if (elementToSpawn != null)
                {
                    float yPos = 1f;

                    if (elementType == ElementType.Ground || elementType == ElementType.Target)
                    {
                        yPos = 0f;
                    }

                    GameObject newElem = null;

                    if (elementType != ElementType.Ground && elementType != ElementType.Air && elementType != ElementType.Target)
                    {
                        newElem = Instantiate(MapElementsManager.CurrentManager.GetMapElement(ElementType.Ground, currentMap.biomeType), new Vector3(pos.x, 0f, -y), Quaternion.identity, MapRoot);
                        StartCoroutine(NewElementAnimation(newElem.transform));
                        allCreatedElements.Add(newElem);
                    }

                    if(elementType == ElementType.DoneTarget || elementType == ElementType.PlayerOnTarget)
                    {
                        newElem = Instantiate(MapElementsManager.CurrentManager.GetMapElement(ElementType.Target, currentMap.biomeType), new Vector3(pos.x, 0f, -y), Quaternion.identity, MapRoot);
                        StartCoroutine(NewElementAnimation(newElem.transform));
                        allCreatedElements.Add(newElem);
                    }

                    newElem = Instantiate(elementToSpawn, new Vector3(pos.x, yPos, -y), Quaternion.identity, MapRoot);
                    allCreatedElements.Add(newElem);
                    StartCoroutine(NewElementAnimation(newElem.transform));

                    if (elementType == ElementType.DoneTarget)
                        newElem.GetComponent<Box>().EnterTarget();

                    if (elementType != ElementType.Ground && elementType != ElementType.Air)
                        currentElements[y, x] = newElem;
                    else
                        currentElements[y, x] = null;

                    if (elementType == ElementType.Player || elementType == ElementType.PlayerOnTarget)
                        Player = currentElements[y, x];
                }

                pos += new Vector3(1, 0, 0);

                if (!skipCreateAnimation && elementToSpawn != null)
                    yield return new WaitForSeconds(createElementDelay);
            }
            pos = new Vector3(0, 0, -pos.z);
        }

        if (!mapDecorator.AreDecorationsSpawned)
            mapDecorator.SpawnDecoration(Biomes.Grass, currentMap.mapSize, 4, 0, false);

        if (!skipCreateAnimation)
            yield return new WaitForSeconds(1f);

        CanMove = true;
    }

    private IEnumerator ClearMapCoroutine(bool removeDecorations)
    {
        CanMove = false;

        for (int i = 0; i < allCreatedElements.Count; i++)
        {
            StartCoroutine(ClearElementAnimation(allCreatedElements[i].transform));
            yield return new WaitForSeconds(destroyElementDelay);
        }

        if (removeDecorations)
            mapDecorator.ClearSpawnedDecorations();

        allCreatedElements.Clear();
        currentMap = null;
        currentElements = null;
    }

    private IEnumerator NewElementAnimation(Transform obj)
    {
        if (!skipCreateAnimation)
        {
            Vector3 finalScale = obj.localScale;
            obj.localScale = Vector3.zero;
            float t = 0f;

            while (obj.localScale.x < finalScale.x)
            {
                t += Time.deltaTime * createAnimationUpScaleSpeed;
                Vector3 scale = Vector3.Lerp(obj.localScale, Vector3.one * createAnimationScale, Time.deltaTime * createAnimationUpScaleSpeed);
                obj.localScale = scale;
                yield return null;
            }

            obj.localScale = finalScale;
        }
    }

    private IEnumerator ClearElementAnimation(Transform obj)
    {
        float t = 0f;

        while (obj.localScale.x > 0.025f)
        {
            t += Time.deltaTime * createAnimationDownScaleSpeed;
            Vector3 scale = Vector3.Lerp(obj.localScale, Vector3.zero, Time.deltaTime * createAnimationDownScaleSpeed);
            obj.localScale = scale;
            yield return null;
        }

        Destroy(obj.gameObject);
    }

    /// <summary>
    /// Moves player to new position. Returns true if player has been moved correctly.
    /// </summary>
    public bool MovePlayer(Vector2Int newPos, Vector2Int dir)
    {
        if (!CanMove) return false;

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

                StartCoroutine(SmoothMoveElement(Player.transform, new Vector3(newPos.x, 1f, -newPos.y)));
                StartCoroutine(SmoothMoveElement(currentElements[newPos.y, newPos.x].transform, new Vector3(newBoxPos.x, 1f, -newBoxPos.y)));

                if(GetElementType(newPos) == ElementType.DoneTarget)
                {
                    Debug.Log("<color=red>EXIT TARGET</color>");
                    currentElements[newPos.y, newPos.x].GetComponent<Box>().ExitTarget();
                }

                if (GetElementType(newBoxPos) == ElementType.Target)
                {
                    Debug.Log("<color=green>ENTER TARGET</color>");
                    currentElements[newPos.y, newPos.x].GetComponent<Box>().EnterTarget();
                }

                currentMap.mapDefinition[oldPlayerPos.y, oldPlayerPos.x] = 
                    GetElementType(oldPlayerPos) == ElementType.PlayerOnTarget ? ElementType.Target : ElementType.Ground;

                currentMap.mapDefinition[newPos.y, newPos.x] = 
                    GetElementType(newPos) == ElementType.DoneTarget ? ElementType.PlayerOnTarget : ElementType.Player;

                currentMap.mapDefinition[newBoxPos.y, newBoxPos.x] = 
                    GetElementType(newBoxPos) == ElementType.Target ? ElementType.DoneTarget : ElementType.Box;

                currentElements[oldPlayerPos.y, oldPlayerPos.x] = null;
                currentElements[newPos.y, newPos.x] = Player;
                currentElements[newBoxPos.y, newBoxPos.x] = box;

                if (OnPlayerMove != null) OnPlayerMove.Invoke(newPos);
                return true;
            }
            else
            {
                return false;
            }
        }

        print($"old player pos = {oldPlayerPos}");
        print($"new pos = {newPos}");

        StartCoroutine(SmoothMoveElement(Player.transform, new Vector3(newPos.x, 1f, -newPos.y)));

        currentMap.mapDefinition[oldPlayerPos.y, oldPlayerPos.x] = 
            GetElementType(oldPlayerPos) == ElementType.PlayerOnTarget ? ElementType.Target : ElementType.Ground;
        currentMap.mapDefinition[newPos.y, newPos.x] = GetElementType(newPos) == ElementType.Target ? ElementType.PlayerOnTarget : ElementType.Player;

        currentElements[oldPlayerPos.y, oldPlayerPos.x] = null;
        currentElements[newPos.y, newPos.x] = Player;

        if (OnPlayerMove != null) OnPlayerMove.Invoke(newPos);
        return true;
    }

    /// <summary>
    /// Smoothly moves given element to given position.
    /// </summary>
    private IEnumerator SmoothMoveElement(Transform element, Vector3 newPos)
    {
        CanMove = false;

        while (element.position != newPos) 
        {
            Vector3 pos = Vector3.MoveTowards(element.transform.position, newPos, Time.deltaTime * moveSpeed);
            element.transform.position = pos;
            yield return null;
        }

        element.position = new Vector3(Mathf.RoundToInt(element.position.x), element.position.y, Mathf.RoundToInt(element.position.z));

        if (!currentMap.IsAllGoalsDone && !LevelManager.CurrentManager.IsPaused)
            CanMove = true;
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
}
