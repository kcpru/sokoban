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
    [SerializeField] private MainMenu mainMenu;

    [Header("UI")]
    [SerializeField] private Animator winScreenModule1;
    [SerializeField] private Animator winScreenModule2;
    [SerializeField] private Animator levelUI;
    [SerializeField] private Animator pauseMenu;

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
    public bool IsPaused { get; private set; }
    public Difficulty CurrentDifficulty { get; private set; }
    public bool IsPlaying => CurrentMap != null;
    public Map CurrentMap => MapManager.CurrentMapManager.currentMap;

    public int Points // CHUJOWY WZÓR - NOWY TRZEBA ZROBIÆ
    {
        get
        {
            int allTargets = CurrentMap.ElementCount(ElementType.Target) + CurrentMap.ElementCount(ElementType.DoneTarget) + CurrentMap.ElementCount(ElementType.PlayerOnTarget);
            int count = Mathf.RoundToInt(100 * (allTargets * 10) * (CurrentMap.ElementCount(ElementType.DoneTarget) / (float)allTargets) / (CurrentMovesCount / 4f));

            return count;
        }
    }

    public static LevelManager CurrentManager { get; protected set; }

    private Color targetBackgroundColor;
    private TextMeshPro movesText;

    private void Awake() => CurrentManager = this;

    private void Start()
    {
        targetBackgroundColor = mainMenuColor;
        MapManager.CurrentMapManager.OnPlayerMove += OnPlayerMove;

        movesText = levelUI.transform.GetChild(0).GetComponent<TextMeshPro>();
    }

    private void Update()
    {
        cam.backgroundColor = Color.Lerp(cam.backgroundColor, targetBackgroundColor, Time.deltaTime * changeColorSpeed);

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            SwitchPauseMenu(!IsPaused);
        }
    }

    private void OnPlayerMove(Vector2Int obj)
    {
        CurrentMovesCount++;
        movesText.text = CurrentMovesCount.ToString();

        if (CurrentMap.IsAllGoalsDone)
        {
            StartCoroutine(Win());

            IEnumerator Win()
            {
                print("<color=green>WIN</color>");

                yield return new WaitForSeconds(1f);

                levelUI.SetBool("show", false);
                movesText.text = "0";

                SaveRecord();

                RankingManager.Record[] records = RankingManager.GetRecords(CurrentMap.name);
                TextMeshPro text = MainMenu.ModuleNumber == 2 ? winScreenModule2.transform.GetChild(0).GetChild(0).GetComponent<TextMeshPro>() :
                    winScreenModule1.transform.GetChild(0).GetChild(0).GetComponent<TextMeshPro>();
                text.text = "";

                foreach (RankingManager.Record r in records)
                {
                    text.text += $"Points: <b>{r.points.ToString()} | </b>Count of moves: <b>{r.moves.ToString()}</b> | Date: <b>{r.date.ToShortDateString()} {r.date.ToShortTimeString()}</b>\n";
                }

                MapManager.CurrentMapManager.ClearMap();
                yield return new WaitForSeconds(
                    (MapManager.CurrentMapManager.allCreatedElements.Count * MapManager.CurrentMapManager.destroyElementDelay) + 1f);

                if (MainMenu.ModuleNumber == 1)
                    winScreenModule1.SetBool("show", true);
                else if (MainMenu.ModuleNumber == 2)
                    winScreenModule2.SetBool("show", true);

                cam.transform.rotation = Quaternion.identity;

                CurrentMovesCount = 0;
            }
        }
    }

    public void NextLevel()
    {
        if (!winScreenModule1.GetBool("show")) return;
        winScreenModule1.SetBool("show", false);

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

    public void FinishLevel()
    {
        if (!IsPlaying) return;

        if (CurrentMovesCount > 0)
            SaveRecord();

        BackToMenu();
    }

    public void BackToMenu()
    {
        StartCoroutine(BackToMenuCoroutine());

        IEnumerator BackToMenuCoroutine()
        {
            movesText.text = "0";
            CurrentMovesCount = 0;

            levelUI.SetBool("show", false);
            SwitchPauseMenu(false);

            if(CurrentMap != null)
            {
                MapManager.CurrentMapManager.ClearMap();

                yield return new WaitForSeconds(
                    (MapManager.CurrentMapManager.allCreatedElements.Count * MapManager.CurrentMapManager.destroyElementDelay) + 1f);
            }

            cam.transform.rotation = Quaternion.identity;
            winScreenModule1.SetBool("show", false);
            winScreenModule2.SetBool("show", false);

            yield return new WaitForSeconds(0.5f);

            SetMainMenuBackgroundColor();
            Animator camAnim = cam.GetComponent<Animator>();
            camAnim.enabled = true;
            mainMenu.OpenPage(MainMenu.ModuleNumber == 2 ? 2 : 0);
            camAnim.SetTrigger("switch");
        }
    }

    public void BackToMenuAndSaveProgress()
    {
        SaveProgress();
        BackToMenu();
    }

    private void SaveProgress()
    {
        Debug.Log("TODO: Save operations");
    }

    private void SaveRecord()
    {
        RankingManager.Record record =
                    new RankingManager.Record(CurrentMap.name, CurrentMovesCount, Points, System.DateTime.Now);
        RankingManager.AddRecord(record);
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

    public void SwitchPauseMenu(bool isOn)
    {
        if (!IsPlaying || (isOn && !MapManager.CurrentMapManager.CanMove)) return;

        pauseMenu.SetBool("show", isOn);
        MapManager.CurrentMapManager.CanMove = !isOn;
        IsPaused = isOn;
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
