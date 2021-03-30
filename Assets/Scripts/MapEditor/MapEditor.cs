using System.Collections.Generic;
using UnityEngine;
using System.IO;

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

    public ElementType CurrentlySelectedElement { get; private set; } = ElementType.Ground;
    public bool IsDeleting { get; private set; } = false;

    public static bool IsEditor { get; private set; } = false;
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

    public void InitializeEditor(Map mapToLoad, string path)
    {
        mapPath = path;
        InitializeEditor(mapToLoad.mapDefinition, mapToLoad.mapSize, mapToLoad.biomeType, mapToLoad.difficulty, mapToLoad.name);
    }

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

    public void SelectElement(ElementType elementType) => CurrentlySelectedElement = elementType;

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

    private void CameraSettings()
    {
        int max = mapSize.x > mapSize.y ? mapSize.x : mapSize.y;
        cam.transform.position = new Vector3((mapSize.x / 2f) - 1f, max * 0.625f, -mapSize.y - 1f);
        cam.transform.LookAt(new Vector3(mapSize.x / 2f, 0f, mapSize.y * -0.5f));
        cam.transform.eulerAngles = new Vector3(cam.transform.eulerAngles.x + 5f, 0f, 0f);
        cam.GetComponent<MapEditorCamera>().ActivateController(mapSize);
    }

    private void MakeMapMiniature()
    {
        CameraSettings();

        cam.transform.GetChild(0).GetComponent<Camera>().enabled = false;
        Screenshotter.TakeScreenshot(Screenshotter.GetMapIconPath(mapPath), cam);
        cam.transform.GetChild(0).GetComponent<Camera>().enabled = true;
    }

    public static string[] GetAllMapsPaths()
    {
        if (!Directory.Exists(PathToMapsDir)) return null;

        string[] allFiles = Directory.GetFiles(PathToMapsDir);
        List<string> xmlFiles = new List<string>();

        foreach(string file in allFiles)
        {
            if (Path.GetExtension(file) == ".xml")
                xmlFiles.Add(file);
        }

        return xmlFiles.ToArray();
    }

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

    public void SwitchBiome()
    {
        Biomes[] arr = (Biomes[])System.Enum.GetValues(typeof(Biomes));
        int j = System.Array.IndexOf(arr, biomeType) + 1;
        Biomes nextBiome = (arr.Length == j) ? arr[0] : arr[j];
        biomeType = nextBiome;
        LevelManager.CurrentManager.SetBackgroundColor(biomeType);
        ResizeGrid(mapSize);
    }

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
