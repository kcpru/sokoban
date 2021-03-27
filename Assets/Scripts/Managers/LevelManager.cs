using UnityEngine;
using System.Collections;
using TMPro;

/// <summary>
/// Class that manages level.
/// </summary>
public class LevelManager : MonoBehaviour
{
    [Header("Common settings")]
    [SerializeField] private Camera cam;

    [Header("UI")]
    [SerializeField] private Animator winScreen;
    [SerializeField] private Animator levelUI;

    [Header("Module 1")]
    [SerializeField] private TextAsset[] easyLevels;
    [SerializeField] private TextAsset[] mediumLevels;
    [SerializeField] private TextAsset[] hardLevels;

    [Header("Background colors")]
    [SerializeField] private Color mainMenuColor;
    [SerializeField] private Color grassBiomeColor;
    [SerializeField] private Color desertBiomeColor;
    [SerializeField] private Color winterBiomeColor;
    [SerializeField] private Color rockBiomeColor;
    [SerializeField] private Color lavaBiomeColor;
    [SerializeField] private float changeColorSpeed = 10f;

    public int CurrentMovesCount { get; private set; }
    public Difficulty CurrentDifficulty { get; private set; }

    public static LevelManager CurrentManager { get; protected set; }

    private Color targetBackgroundColor;
    private TextMeshPro movesText;
    private TextMeshPro timeText;

    private float timer = 0f;
    private bool timerOn = false;

    public float CurrentTime => (float)System.Math.Round(timer, 2);

    private void Awake() => CurrentManager = this;

    private void Start()
    {
        targetBackgroundColor = mainMenuColor;
        MapManager.CurrentMapManager.OnPlayerMove += OnPlayerMove;

        movesText = levelUI.transform.GetChild(0).GetComponent<TextMeshPro>();
        timeText = levelUI.transform.GetChild(1).GetComponent<TextMeshPro>();
    }

    private void Update()
    {
        cam.backgroundColor = Color.Lerp(cam.backgroundColor, targetBackgroundColor, Time.deltaTime * changeColorSpeed);

        if (timerOn)
        {
            timer += Time.deltaTime;
            string s = ((float)System.Math.Round(timer, 2)).ToString();
            s = s.Replace(',', ':');
            timeText.text = s;
        }
    }

    private void OnPlayerMove(Vector2Int obj)
    {
        CurrentMovesCount++;
        movesText.text = CurrentMovesCount.ToString();

        if (CurrentMovesCount == 1)
            StartTimer();

        if (MapManager.CurrentMapManager.currentMap.IsAllGoalsDone)
        {
            StartCoroutine(Win());

            IEnumerator Win()
            {
                print("<color=green>WIN</color>");

                StopTimer();

                yield return new WaitForSeconds(1f);

                levelUI.SetBool("show", false);
                movesText.text = "0";
                timeText.text = "0:0";

                RankingManager.AddRecord(MapManager.CurrentMapManager.currentMap, CurrentTime, CurrentMovesCount);

                RankingManager.Record[] records = RankingManager.GetRecords(MapManager.CurrentMapManager.currentMap.name); 
                TextMeshPro text = winScreen.transform.GetChild(0).GetChild(0).GetComponent<TextMeshPro>();
                text.text = "";

                foreach (RankingManager.Record record in records)
                {
                    text.text += $"Count of moves: <b>{record.moves}</b> | Time: <b>{record.time}</b>\n";
                }

                MapManager.CurrentMapManager.ClearMap();
                yield return new WaitForSeconds(
                    (MapManager.CurrentMapManager.allCreatedElements.Count * MapManager.CurrentMapManager.destroyElementDelay) + 1f);

                winScreen.SetBool("show", true);
                cam.transform.rotation = Quaternion.identity;

                timer = 0f;
                CurrentMovesCount = 0;
            }
        }
    }

    private void StartTimer()
    {
        timer = 0f;
        timerOn = true;
    }

    private void StopTimer() => timerOn = false;

    public void NextLevel()
    {
        if (!winScreen.GetBool("show")) return;
        winScreen.SetBool("show", false);

        StartCoroutine(NextLevelCoroutine());

        IEnumerator NextLevelCoroutine()
        {
            yield return new WaitForSeconds(1f);

            MapSerializer serializer = new MapSerializer(MapSerializer.MapsPath + "/" + RandomMap(CurrentDifficulty).name + ".xml");
            Map deserializedMap = serializer.Deserialize();

            SetBackgroundColor(deserializedMap.biomeType);
            LoadLevel(deserializedMap);
        }
    }

    public void BackToMenu()
    {
        StartCoroutine(BackToMenuCoroutine());

        IEnumerator BackToMenuCoroutine()
        {
            winScreen.SetBool("show", false);

            yield return new WaitForSeconds(0.5f);

            SetMainMenuBackgroundColor();
            Animator camAnim = cam.GetComponent<Animator>();
            camAnim.enabled = true;
            camAnim.SetInteger("view", MainMenu.MAIN_MENU);
            camAnim.SetTrigger("switch");
        }
    }

    /// <summary>
    /// Loads given <seealso cref="Map"/> and sets correct camera position.
    /// </summary>
    /// <param name="map"><seealso cref="Map"/> to load</param>
    public void LoadLevel(Map map)
    {
        levelUI.SetBool("show", true);
        MapManager.CurrentMapManager.CreateMap(map);

        int max = map.mapSize.x > map.mapSize.y ? map.mapSize.x : map.mapSize.y;
        cam.transform.position = new Vector3((map.mapSize.x / 2f) - 1f, max * 0.625f, -map.mapSize.y - 1f);
        cam.transform.LookAt(new Vector3(map.mapSize.x / 2f, 0f, map.mapSize.y * -0.5f));
        cam.transform.eulerAngles = new Vector3(cam.transform.eulerAngles.x + 5f, 0f, 0f);

        CurrentDifficulty = map.difficulty;
    }

    /// <summary>
    /// Sets camera background color fits to given <seealso cref="Biomes"/>
    /// </summary>
    public void SetBackgroundColor(Biomes biome)
    {
        switch (biome)
        {
            case Biomes.Grass:
                targetBackgroundColor = grassBiomeColor;
                break;
            case Biomes.Desert:
                targetBackgroundColor = desertBiomeColor;
                break;
            case Biomes.Winter:
                targetBackgroundColor = winterBiomeColor;
                break;
            case Biomes.Rock:
                targetBackgroundColor = rockBiomeColor;
                break;
            case Biomes.Lava:
                targetBackgroundColor = lavaBiomeColor;
                break;
        }
    }

    /// <summary>
    /// Sets camera background color in main menu.
    /// </summary>
    public void SetMainMenuBackgroundColor() => targetBackgroundColor = mainMenuColor;

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
