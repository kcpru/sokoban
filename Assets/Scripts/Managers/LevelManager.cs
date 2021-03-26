using UnityEngine;

/// <summary>
/// Class that manages level.
/// </summary>
public class LevelManager : MonoBehaviour
{
    [Header("Module 1")]
    [SerializeField] private TextAsset[] easyLevels;
    [SerializeField] private TextAsset[] mediumLevels;
    [SerializeField] private TextAsset[] hardLevels;

    public int CurrentMovesCount { get; private set; }

    public static LevelManager CurrentManager { get; protected set; }

    private void Awake() => CurrentManager = this;

    private void Start() => MapManager.CurrentMapManager.OnPlayerMove += OnPlayerMove;

    private void OnPlayerMove(Vector2Int obj)
    {
        CurrentMovesCount++;

        if (MapManager.CurrentMapManager.currentMap.IsAllGoalsDone)
        {
            print("<color=green>WIN</color>");
            MapManager.CurrentMapManager.ClearMap();
        }
    }

    /// <summary>
    /// Loads given <seealso cref="Map"/> and sets correct camera position.
    /// </summary>
    /// <param name="map"><seealso cref="Map"/> to load</param>
    /// <param name="camera">Camera to operate on</param>
    public void LoadLevel(Map map, Transform camera)
    {
        MapManager.CurrentMapManager.CreateMap(map);

        int max = map.mapSize.x > map.mapSize.y ? map.mapSize.x : map.mapSize.y;
        camera.position = new Vector3((map.mapSize.x / 2f) - 1f, max * 0.625f, -map.mapSize.y - 1f);
        camera.LookAt(new Vector3(map.mapSize.x / 2f, 0f, map.mapSize.y * -0.5f));
        camera.eulerAngles = new Vector3(camera.eulerAngles.x + 5f, 0f, 0f);
    }

    /// <summary>
    /// Returns random map with given <seealso cref="Difficulty"/>
    /// </summary>
    public TextAsset RandomMap(Difficulty difficulty)
    {
        TextAsset map = null;

        if (difficulty == Difficulty.Easy)
        {
            int randomMap = Random.Range(0, easyLevels.Length);
            map = easyLevels[randomMap];
        }
        else if (difficulty == Difficulty.Medium)
        {
            int randomMap = Random.Range(0, mediumLevels.Length);
            map = mediumLevels[randomMap];
        }
        else if (difficulty == Difficulty.Hard)
        {
            int randomMap = Random.Range(0, hardLevels.Length);
            map = hardLevels[randomMap];
        }

        return map;
    }
}
