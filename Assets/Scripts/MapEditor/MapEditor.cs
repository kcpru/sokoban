using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class MapEditor : MonoBehaviour
{
    [Header("Misc")]
    [SerializeField] private Camera cam;
    [SerializeField] private Animator editorUI;
    [SerializeField] private MapEditorErrorMsg errorMsg;

    [Header("Objects")]
    [SerializeField] private GameObject gridField;

    private GameObject[,] grid;
    private List<GameObject>[,] elements;
    private ElementType[,] elementTypes;

    private Transform GridRoot => transform.GetChild(0);
    private Transform MapElementsRoot => transform.GetChild(1);

    private GameObject[] allButtons;
    private bool isPlayer = false;

    public ElementType CurrentlySelectedElement { get; private set; } = ElementType.Ground;
    public bool IsDeleting { get; private set; } = false;

    public static bool IsEditor { get; private set; } = false;
    public static string PathToMapsDir => Path.Combine(Application.dataPath, "MyMaps");

    private void Start()
    {
        allButtons = new GameObject[editorUI.transform.GetChild(0).GetChild(0).childCount];
        for (int i = 0; i < allButtons.Length; i++)
            allButtons[i] = editorUI.transform.GetChild(0).GetChild(0).GetChild(i).gameObject;
    }

    public void InitializeEditor(Vector2Int gridSize)
    {
        ElementType[,] elements = new ElementType[gridSize.y, gridSize.x];

        for(int y = 0; y < gridSize.y; y++)
        {
            for (int x = 0; x < gridSize.x; x++)
                elements[y, x] = ElementType.Air;
        }

        InitializeEditor(new Map("new map", elements));
    }

    public void InitializeEditor(Map mapToLoad)
    {
        if (!Directory.Exists(PathToMapsDir))
            Directory.CreateDirectory(PathToMapsDir);

        IsEditor = true;

        int max = mapToLoad.mapSize.x > mapToLoad.mapSize.y ? mapToLoad.mapSize.x : mapToLoad.mapSize.y;
        cam.transform.position = new Vector3((mapToLoad.mapSize.x / 2f) - 1f, max * 0.625f, -mapToLoad.mapSize.y - 1f);
        cam.transform.LookAt(new Vector3(mapToLoad.mapSize.x / 2f, 0f, mapToLoad.mapSize.y * -0.5f));
        cam.transform.eulerAngles = new Vector3(cam.transform.eulerAngles.x + 5f, 0f, 0f);
        cam.GetComponent<MapEditorCamera>().ActivateController(mapToLoad.mapSize);

        editorUI.SetBool("show", true);
        CreateGrid(mapToLoad.mapSize);

        elements = new List<GameObject>[mapToLoad.mapSize.y, mapToLoad.mapSize.x];
        elementTypes = new ElementType[mapToLoad.mapSize.y, mapToLoad.mapSize.x];

        for (int y = 0; y < mapToLoad.mapSize.y; y++)
        {
            for (int x = 0; x < mapToLoad.mapSize.x; x++)
                elementTypes[y, x] = ElementType.Air;
        }

        for (int y = 0; y < mapToLoad.mapSize.y; y++)
        {
            for(int x = 0; x < mapToLoad.mapSize.x; x++)
            {
                elements[y, x] = new List<GameObject>();
                
                SelectElement(mapToLoad.mapDefinition[y, x]);
                PlaceElement(new Vector3(x, 0f, -y));
            }
        }

        LevelManager.CurrentManager.SetBackgroundColor(mapToLoad.biomeType);
        SelectElement(allButtons[2].GetComponent<Button3D>());
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
        if(isPlayer && (CurrentlySelectedElement == ElementType.Player || CurrentlySelectedElement == ElementType.PlayerOnTarget) && !IsDeleting)
        {
            errorMsg.SetText("Cannot place more than one player element!", 4f);
            return;
        }

        if (IsDeleting)
        {
            if (elements[Mathf.Abs((int)pos.z), Mathf.Abs((int)pos.x)].Count > 0)
            {
                RemoveAllElementsFromArray(Mathf.Abs((int)pos.x), Mathf.Abs((int)pos.z));
            }

            return;
        }

        if (elementTypes[Mathf.Abs((int)pos.z), Mathf.Abs((int)pos.x)] == CurrentlySelectedElement || CurrentlySelectedElement == ElementType.Air)
        {
            return;
        }

        if (elements[Mathf.Abs((int)pos.z), Mathf.Abs((int)pos.x)].Count > 0)
        {
            RemoveAllElementsFromArray(Mathf.Abs((int)pos.x), Mathf.Abs((int)pos.z));
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
                Instantiate(MapManager.CurrentMapManager.GetMapElement(ElementType.Ground), new Vector3(pos.x, 0f, pos.z), Quaternion.identity, MapElementsRoot);
            elements[Mathf.Abs((int)pos.z), Mathf.Abs((int)pos.x)].Add(newElem);
            Destroy(newElem.GetComponent<Collider>());
        }

        if (CurrentlySelectedElement == ElementType.DoneTarget || CurrentlySelectedElement == ElementType.PlayerOnTarget)
        {
            newElem = 
                Instantiate(MapManager.CurrentMapManager.GetMapElement(ElementType.Target), new Vector3(pos.x, 0f, pos.z), Quaternion.identity, MapElementsRoot);
            elements[Mathf.Abs((int)pos.z), Mathf.Abs((int)pos.x)].Add(newElem);
            Destroy(newElem.GetComponent<Collider>());
        }

        if (newElem != null)
            Destroy(newElem.GetComponent<Collider>());

        newElem = Instantiate(MapManager.CurrentMapManager.GetMapElement(
                CurrentlySelectedElement), new Vector3(pos.x, yPos, pos.z), Quaternion.identity, MapElementsRoot);

        if(CurrentlySelectedElement == ElementType.PlayerOnTarget)
        {
            Color c = newElem.GetComponent<Renderer>().material.color;
            newElem.GetComponent<Renderer>().material.color = new Color(c.r, c.g, c.b, 0.825f);
        }

        if(CurrentlySelectedElement == ElementType.DoneTarget)
        {
            newElem.GetComponent<Renderer>().material = MapManager.CurrentMapManager.targetDoneMaterial;
        }

        elements[Mathf.Abs((int)pos.z), Mathf.Abs((int)pos.x)].Add(newElem);
        elementTypes[Mathf.Abs((int)pos.z), Mathf.Abs((int)pos.x)] = CurrentlySelectedElement;

        Destroy(newElem.GetComponent<Collider>());

        if (CurrentlySelectedElement == ElementType.Player || CurrentlySelectedElement == ElementType.PlayerOnTarget)
            isPlayer = true;

        print("p");
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

    private void RemoveAllElementsFromArray(int x, int y)
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
}
