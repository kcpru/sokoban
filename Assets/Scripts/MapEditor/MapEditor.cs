using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapEditor : MonoBehaviour
{
    [Header("Misc")]
    [SerializeField] private Camera cam;
    [SerializeField] private Animator editorUI;

    [Header("Objects")]
    [SerializeField] private GameObject gridField;

    private GameObject[,] grid;
    private List<GameObject>[,] elements;
    private ElementType[,] elementTypes;

    private Transform GridRoot => transform.GetChild(0);
    private Transform MapElementsRoot => transform.GetChild(1);

    public ElementType CurrentlySelectedElement { get; private set; } = ElementType.Ground;
    public bool IsDeleting { get; private set; } = false;

    public static bool IsEditor { get; private set; } = false;

    public void InitializeEditor(Vector2Int gridSize)
    {
        IsEditor = true;

        int max = gridSize.x > gridSize.y ? gridSize.x : gridSize.y;
        cam.transform.position = new Vector3((gridSize.x / 2f) - 1f, max * 0.625f, -gridSize.y - 1f);
        cam.transform.LookAt(new Vector3(gridSize.x / 2f, 0f, gridSize.y * -0.5f));
        cam.transform.eulerAngles = new Vector3(cam.transform.eulerAngles.x + 5f, 0f, 0f);
        cam.GetComponent<MapEditorCamera>().ActivateController(gridSize);

        editorUI.SetBool("show", true);
        CreateGrid(gridSize);

        elements = new List<GameObject>[gridSize.y, gridSize.x];
        elementTypes = new ElementType[gridSize.y, gridSize.x];

        for(int y = 0; y < gridSize.y; y++)
        {
            for(int x = 0; x < gridSize.x; x++)
            {
                elements[y, x] = new List<GameObject>();
                elementTypes[y, x] = ElementType.Air;
            }
        }
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
        if(sender.name == "Delete")
        {
            IsDeleting = true;
            return;
        }
        else
        {
            IsDeleting = false;
        }

        ElementType elementType = (ElementType)System.Enum.Parse(typeof(ElementType), sender.name);
        CurrentlySelectedElement = elementType;
    }

    public void PlaceElement(Vector3 pos)
    {
        print(elementTypes[Mathf.Abs((int)pos.z), Mathf.Abs((int)pos.x)].ToString());

        if (IsDeleting)
        {
            if (elements[Mathf.Abs((int)pos.z), Mathf.Abs((int)pos.x)].Count > 0)
            {
                RemoveAllElementsFromArray(Mathf.Abs((int)pos.x), Mathf.Abs((int)pos.z));
            }

            return;
        }

        if (elementTypes[Mathf.Abs((int)pos.z), Mathf.Abs((int)pos.x)] == CurrentlySelectedElement)
            return;

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
    }

    private void RemoveAllElementsFromArray(int x, int y)
    {
        elementTypes[y, x] = ElementType.Air;

        foreach(GameObject obj in elements[y, x])
        {
            Destroy(obj);
        }

        elements[y, x].Clear();
    }
}
