using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// Map editor allows to create own maps, save them and then play on them.
/// </summary>
public class MapEditor : MonoBehaviour
{
    [Header("Misc")]
    [SerializeField] private Camera cam;
    [SerializeField] private Animator editorUI;
    [SerializeField] private MapEditorMsg msg;
    [SerializeField] private MainMenu mainMenu;

    [Header("Objects")]
    [SerializeField] private GameObject gridField;

    private GameObject[,] grid;
    private List<GameObject>[,] elements;
    private ElementType[,] elementTypes;
    private Vector2Int mapSize;
    private Biomes biomeType;
    private Difficulty difficulty;
    private string mapName = "";
    private string mapPath = "";

    private Transform GridRoot => transform.GetChild(0);
    private Transform MapElementsRoot => transform.GetChild(1);

    private GameObject[] allButtons;
    private GameObject[] buttonsRoots;
    private bool isPlayer = false;
    private InputField3D nameField;

    /// <summary>
    /// Currently selected type of map element, that will be placed on grid.
    /// </summary>
    public ElementType CurrentlySelectedElement { get; private set; } = ElementType.Ground;

    /// <summary>
    /// Indicates whether deleting mode is on.
    /// </summary>
    public bool IsDeleting { get; private set; } = false;

    /// <summary>
    /// Indicates whether the map editor is opened.
    /// </summary>
    public static bool IsEditor { get; private set; } = false;

    /// <summary>
    /// Returns path to folder that contains user's maps.
    /// </summary>
    public static string PathToMapsDir => Path.Combine(Application.dataPath, "MyMaps");

    private void Start()
    {
        allButtons = new GameObject[editorUI.transform.GetChild(0).GetChild(0).childCount * 5];
        buttonsRoots = new GameObject[5];
        nameField = editorUI.transform.GetChild(0).GetChild(13).GetComponent<InputField3D>();
        nameField.OnValueChanged.AddListener(InputFieldChanged);
        int index = 0;

        for (int j = 0; j < 5; j++)
        {
            buttonsRoots[j] = editorUI.transform.GetChild(0).GetChild(j).gameObject;
            for (int i = 0; i < allButtons.Length / 5; i++)
            {
                allButtons[index] = editorUI.transform.GetChild(0).GetChild(j).GetChild(i).gameObject;
                index++;
            }
        }
    }

    /// <summary>
    /// Initializes new editor session with empty grid.
    /// </summary>
    /// <param name="gridSize">Size of grid.</param>
    public void InitializeEditor(Vector2Int gridSize)
    {
        ElementType[,] elements = new ElementType[gridSize.y, gridSize.x];

        for(int y = 0; y < gridSize.y; y++)
        {
            for (int x = 0; x < gridSize.x; x++)
                elements[y, x] = ElementType.Air;
        }

        InitializeEditor(elements, gridSize, biomeType, difficulty, mapPath);
    }

    /// <summary>
    /// Initializes new editor session and loads map with given size with elements from given 2D array.
    /// </summary>
    /// <param name="mapDefinition">Map elements</param>
    /// <param name="mapSize">Size of map</param>
    /// <param name="biomeType">Type of biome</param>
    /// <param name="difficulty">Difficulty of map</param>
    /// <param name="mapName">Name of map</param>
    public void InitializeEditor(ElementType[,] mapDefinition, Vector2Int mapSize, Biomes biomeType, Difficulty difficulty, string mapName)
    {
        if (!Directory.Exists(PathToMapsDir))
            Directory.CreateDirectory(PathToMapsDir);

        IsEditor = true;
        this.mapSize = mapSize;
        this.mapName = mapName;
        this.biomeType = biomeType;
        nameField.SetValue(mapName);

        CameraSettings();

        editorUI.SetBool("show", true);
        CreateGrid(mapSize);

        elements = new List<GameObject>[mapSize.y, mapSize.x];
        elementTypes = new ElementType[mapSize.y, mapSize.x];

        for (int y = 0; y < mapSize.y; y++)
        {
            for (int x = 0; x < mapSize.x; x++)
                elementTypes[y, x] = ElementType.Air;
        }

        for (int y = 0; y < mapSize.y; y++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                elements[y, x] = new List<GameObject>();

                SelectElement(mapDefinition[y, x]);
                PlaceElement(new Vector3(x, 0f, -y));
            }
        }

        LevelManager.CurrentManager.SetBackgroundColor(biomeType);
        SelectElement(allButtons[2].GetComponent<Button3D>());
        SetButtons();
    }

    /// <summary>
    /// Initializes new editor session, loads map and reads necessery informations from given <seealso cref="Map"/>.
    /// </summary>
    /// <param name="mapToLoad">Deserialized map to load informations from</param>
    /// <param name="path">Path from which given map was loaded</param>
    public void InitializeEditor(Map mapToLoad, string path)
    {
        mapPath = path;
        InitializeEditor(mapToLoad.mapDefinition, mapToLoad.mapSize, mapToLoad.biomeType, mapToLoad.difficulty, mapToLoad.name);
    }

    /// <summary>
    /// Change size of current grid.
    /// </summary>
    /// <param name="newSize"></param>
    private void ResizeGrid(Vector2Int newSize)
    {
        ClearElements();
        ClearGrid();

        ElementType[,] oldElements = elementTypes;
        ElementType[,] elements = new ElementType[newSize.y, newSize.x];

        for (int y = 0; y < newSize.y; y++)
        {
            for (int x = 0; x < newSize.x; x++)
            {
                if (y < oldElements.GetLength(0) && x < oldElements.GetLength(1))
                {
                    elements[y, x] = oldElements[y, x];
                }
                else
                {
                    elements[y, x] = ElementType.Air;
                }
            }
        }

        for (int y = 0; y < mapSize.y; y++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                elementTypes[y, x] = ElementType.Air;
            }
        }

        isPlayer = false;
        InitializeEditor(elements, newSize, biomeType, Difficulty.Easy, mapName);
    }

    /// <summary>
    /// Creates new grid with given size.
    /// </summary>
    /// <param name="gridSize">Size of new grid.</param>
    private void CreateGrid(Vector2Int gridSize)
    {
        grid = new GameObject[gridSize.y, gridSize.x];

        Vector3 pos = default;

        for(int y = 0; y < gridSize.y; y++)
        {
            for(int x = 0; x < gridSize.x; x++)
            {
                pos = new Vector3(x, -0.55f, -y);
                grid[y, x] = Instantiate(gridField, pos, Quaternion.identity, GridRoot);
                grid[y, x].GetComponent<GridField>().OnClick.AddListener((position) => PlaceElement(position));
            }
        }
    }

    /// <summary>
    /// Removes all grid panels.
    /// </summary>
    private void ClearGrid()
    {
        if (!IsEditor) return;

        for (int y = 0; y < mapSize.y; y++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                Destroy(grid[y, x]);
            }
        }
    }
    
    /// <summary>
    /// Removes all previously created map elements.
    /// </summary>
    private void ClearElements()
    {
        for (int y = 0; y < mapSize.y; y++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                foreach(GameObject obj in elements[y, x])
                {
                    Destroy(obj);
                }

                elements[y, x].Clear();
            }
        }
    }

    /// <summary>
    /// Returns back to menu, note that it doesn't save map.
    /// </summary>
    public void BackToMenu()
    {
        editorUI.SetBool("show", false);
        ClearGrid();
        ClearElements();
        cam.transform.rotation = Quaternion.identity;
        LevelManager.CurrentManager.SetMainMenuBackgroundColor();
        Animator camAnim = cam.GetComponent<Animator>();
        camAnim.enabled = true;
        mainMenu.OpenPage(MainMenu.ModuleNumber == 2 ? 2 : 0);
        camAnim.SetTrigger("switch");

        elementTypes = new ElementType[0, 0];
        elements = new List<GameObject>[0, 0];
        mapSize = Vector2Int.zero;
        isPlayer = false;
        mapName = string.Empty;
        mapPath = string.Empty;

        cam.GetComponent<MapEditorCamera>().DeactivateController();
    }

    /// <summary>
    /// Saves map to xml file. Gets name from 3D input field.
    /// </summary>
    public void SaveMap()
    {
        if(mapName.Length == 0 || mapName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
        {
            msg.SetText("Map name is not correct!", 4f, Color.red);
            return;
        }

        mapPath = PathToMapsDir + "/" + Path.GetFileNameWithoutExtension(mapName) + ".xml";

        Map map = new Map(mapName, elementTypes, biomeType, difficulty);

        if (map.IsMapDefined)
        {
            MapSerializer mapSerializer = new MapSerializer(mapPath);
            mapSerializer.Serialize(map);
            msg.SetText($"Saved map correctly in: {mapPath}", 6f, Color.green);

            MakeMapMiniature();

            RankingManager.RemoveAllRecords(map.name);
            SaveLoadManager.ClearSave(map);
        }
        else
        {
            msg.SetText("Cannot save this map due to some problems!", 4f, Color.red);
        }
    }

    /// <summary>
    /// Selects element to paint.
    /// </summary>
    /// <param name="sender">Button that sent this request. It is used to get name of element.</param>
    public void SelectElement(MonoBehaviour sender)
    {
        foreach (GameObject btn in allButtons)
            btn.transform.localScale = Vector3.one;

        sender.transform.localScale = sender.transform.localScale * 1.2f;

        if (sender.name == "Delete")
        {
            IsDeleting = true;
            return;
        }
        else
        {
            IsDeleting = false;
        }

        ElementType elementType = (ElementType)System.Enum.Parse(typeof(ElementType), sender.name);
        SelectElement(elementType);
    }

    /// <summary>
    /// Selects element to paint.
    /// </summary>
    public void SelectElement(ElementType elementType) => CurrentlySelectedElement = elementType;

    /// <summary>
    /// Creates currently selected element at given position, replaces it or remove it if IsDeleting is true.
    /// </summary>
    /// <param name="pos">Position on which element will be modified.</param>
    public void PlaceElement(Vector3 pos)
    {
        Collider[] colliders = new Collider[0];

        if (isPlayer && (CurrentlySelectedElement == ElementType.Player || CurrentlySelectedElement == ElementType.PlayerOnTarget) && !IsDeleting)
        {
            msg.SetText("Cannot place more than one player element!", 4f, Color.red);
            return;
        }

        if (IsDeleting)
        {
            if (elements[Mathf.Abs((int)pos.z), Mathf.Abs((int)pos.x)].Count > 0)
            {
                RemoveAllElementsFromList(Mathf.Abs((int)pos.x), Mathf.Abs((int)pos.z));
            }

            return;
        }

        if (elementTypes[Mathf.Abs((int)pos.z), Mathf.Abs((int)pos.x)] == CurrentlySelectedElement || CurrentlySelectedElement == ElementType.Air)
        {
            return;
        }

        if (elements[Mathf.Abs((int)pos.z), Mathf.Abs((int)pos.x)].Count > 0)
        {
            RemoveAllElementsFromList(Mathf.Abs((int)pos.x), Mathf.Abs((int)pos.z));
            PlaceElement(pos);
            return;
        }

        float yPos = 1f;

        if (CurrentlySelectedElement == ElementType.Ground || CurrentlySelectedElement == ElementType.Target)
        {
            yPos = 0f;
        }

        GameObject newElem = null;

        if (CurrentlySelectedElement != ElementType.Ground && CurrentlySelectedElement != ElementType.Air && CurrentlySelectedElement != ElementType.Target)
        {
            newElem = 
                Instantiate(MapElementsManager.CurrentManager.GetMapElement(ElementType.Ground, biomeType), new Vector3(pos.x, 0f, pos.z), Quaternion.identity, MapElementsRoot);
            elements[Mathf.Abs((int)pos.z), Mathf.Abs((int)pos.x)].Add(newElem);
            Destroy(newElem.GetComponent<Collider>());
        }

        if (newElem != null)
        {
            colliders = newElem.GetComponentsInChildren<Collider>();
            foreach (Collider c in colliders)
                Destroy(c);
            colliders = new Collider[0];
        }

        if (CurrentlySelectedElement == ElementType.DoneTarget || CurrentlySelectedElement == ElementType.PlayerOnTarget)
        {
            newElem = 
                Instantiate(MapElementsManager.CurrentManager.GetMapElement(ElementType.Target, biomeType), new Vector3(pos.x, 0f, pos.z), Quaternion.identity, MapElementsRoot);
            elements[Mathf.Abs((int)pos.z), Mathf.Abs((int)pos.x)].Add(newElem);
        }

        if (newElem != null)
        {
            colliders = newElem.GetComponentsInChildren<Collider>();
            foreach (Collider c in colliders)
                Destroy(c);
            colliders = new Collider[0];
        }

        newElem = Instantiate(MapElementsManager.CurrentManager.GetMapElement(CurrentlySelectedElement, biomeType), new Vector3(pos.x, yPos, pos.z), Quaternion.identity, MapElementsRoot);

        if(CurrentlySelectedElement == ElementType.DoneTarget)
        {
            newElem.transform.GetChild(1).localScale = Vector3.one * 0.8f;
            newElem.GetComponent<Box>().EnterTarget();
        }

        elements[Mathf.Abs((int)pos.z), Mathf.Abs((int)pos.x)].Add(newElem);
        elementTypes[Mathf.Abs((int)pos.z), Mathf.Abs((int)pos.x)] = CurrentlySelectedElement;

        colliders = newElem.GetComponentsInChildren<Collider>();
        foreach (Collider c in colliders)
            Destroy(c);

        if (CurrentlySelectedElement == ElementType.Player || CurrentlySelectedElement == ElementType.PlayerOnTarget)
            isPlayer = true;
    }

    /// <summary>
    /// Sets start camera position and rotation.
    /// </summary>
    private void CameraSettings()
    {
        int max = mapSize.x > mapSize.y ? mapSize.x : mapSize.y;
        cam.transform.position = new Vector3((mapSize.x / 2f) - 1f, max * 0.625f, -mapSize.y - 1f);
        cam.transform.LookAt(new Vector3(mapSize.x / 2f, 0f, mapSize.y * -0.5f));
        cam.transform.eulerAngles = new Vector3(cam.transform.eulerAngles.x + 5f, 0f, 0f);
        cam.GetComponent<MapEditorCamera>().ActivateController(mapSize);
    }

    /// <summary>
    /// Makes screenshot of current map and save it to JPG file.
    /// </summary>
    private void MakeMapMiniature()
    {
        CameraSettings();

        cam.transform.GetChild(0).GetComponent<Camera>().enabled = false;
        Screenshotter.TakeScreenshot(Screenshotter.GetMapIconPath(mapPath), cam);
        cam.transform.GetChild(0).GetComponent<Camera>().enabled = true;
    }

    /// <summary>
    /// Returns array that contains paths to all created maps which are in MyMaps directory.
    /// </summary>
    /// <returns></returns>
    public static string[] GetAllMapsPaths()
    {
        if (!Directory.Exists(PathToMapsDir)) return new string[0];

        string[] allFiles = Directory.GetFiles(PathToMapsDir);
        List<string> xmlFiles = new List<string>();

        foreach(string file in allFiles)
        {
            if (Path.GetExtension(file) == ".xml")
                xmlFiles.Add(file);
        }

        return xmlFiles.ToArray();
    }

    /// <summary>
    /// Removes all elements at given position.
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    private void RemoveAllElementsFromList(int x, int y)
    {
        if (elementTypes[y, x] == ElementType.Player || elementTypes[y, x] == ElementType.PlayerOnTarget)
            isPlayer = false;

        elementTypes[y, x] = ElementType.Air;

        foreach(GameObject obj in elements[y, x])
        {
            Destroy(obj);
        }

        elements[y, x].Clear();
    }

    /// <summary>
    /// Gets next biome from <seealso cref="Biomes"/> enum and sets is as current.
    /// </summary>
    public void SwitchBiome()
    {
        Biomes[] arr = (Biomes[])System.Enum.GetValues(typeof(Biomes));
        int j = System.Array.IndexOf(arr, biomeType) + 1;
        Biomes nextBiome = (arr.Length == j) ? arr[0] : arr[j];
        biomeType = nextBiome;
        LevelManager.CurrentManager.SetBackgroundColor(biomeType);
        ResizeGrid(mapSize);
    }

    /// <summary>
    /// Activates buttons that fit to current biome, and deactivates rest of buttons.
    /// </summary>
    private void SetButtons()
    {
        foreach (GameObject go in buttonsRoots)
        {
            print(go.name);
            go.SetActive(false);
        }

        switch (biomeType)
        {
            case Biomes.Grass:
                buttonsRoots[0].SetActive(true);
                SelectElement(buttonsRoots[0].transform.GetChild(2).GetComponent<Button3D>());
                break;
            case Biomes.Desert:
                buttonsRoots[1].SetActive(true);
                SelectElement(buttonsRoots[1].transform.GetChild(2).GetComponent<Button3D>());
                break;
            case Biomes.Winter:
                buttonsRoots[2].SetActive(true);
                SelectElement(buttonsRoots[2].transform.GetChild(2).GetComponent<Button3D>());
                break;
            case Biomes.Rock:
                buttonsRoots[3].SetActive(true);
                SelectElement(buttonsRoots[3].transform.GetChild(2).GetComponent<Button3D>());
                break;
            case Biomes.Lava:
                buttonsRoots[4].SetActive(true);
                SelectElement(buttonsRoots[4].transform.GetChild(2).GetComponent<Button3D>());
                break;
        }
    }
    
    /// <summary>
    /// Calls when 3D input field value has been modified.
    /// </summary>
    /// <param name="sender"><seealso cref="MonoBehaviour"/> that raises this callback.</param>
    public void InputFieldChanged(MonoBehaviour sender) => mapName = nameField.value;

    #region Resize grid buttons
    public void AddX()
    {
        if (mapSize.x + 1 <= Map.MAX_X)
            ResizeGrid(new Vector2Int(mapSize.x + 1, mapSize.y));
    }

    public void RemoveX()
    {
        if (mapSize.x - 1 >= Map.MIN_X)
            ResizeGrid(new Vector2Int(mapSize.x - 1, mapSize.y));
    }

    public void AddY()
    {
        if (mapSize.y + 1 <= Map.MAX_Y)
            ResizeGrid(new Vector2Int(mapSize.x, mapSize.y + 1));
    }

    public void RemoveY()
    {
        if (mapSize.y - 1 >= Map.MIN_Y)
            ResizeGrid(new Vector2Int(mapSize.x, mapSize.y - 1));
    }
    #endregion
}
