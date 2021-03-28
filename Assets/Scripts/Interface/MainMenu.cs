using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour
{
    [Header("Interface")]
    [SerializeField] private Animator camAnim;
    [SerializeField] private GameObject module1;
    [SerializeField] private GameObject module2;
    [SerializeField] private GameObject module3;
    [SerializeField] private GameObject credits;
    [SerializeField] private GameObject module2Levels;
    [SerializeField] private GameObject module2Info;

    [Header("Module 2")]
    [SerializeField] private GameObject module2MapButton;
    private List<GameObject> spawnedMapButtons = new List<GameObject>();

    [Header("Module 3")]
    [SerializeField] private MapEditor mapEditor;

    public const int MAIN_MENU = 0;
    public const int BOTTOM_1 = 1;
    public const int LEVEL = 2;
    public const int EDITOR = 3;
    public const int PAUSE = 4;

    public static int ModuleNumber { get; private set; } = 0;

    private Transform camTrans;
    private string currentLevelModule2;

    private IEnumerator spawnModule2ButtonsCor;

    private void Start()
    {
        camTrans = camAnim.transform;
        camAnim.transform.GetChild(0).GetComponent<Camera>().enabled = true;
    }

    /// <summary>
    /// Opens page with given index.
    /// </summary>
    /// <param name="view">0 - main menu, 1 - module 1, 2 - module 2, 3 - module 3, 4 - credits</param>
    public void OpenPage(int view)
    {
        if(module2.activeSelf)
        {
            StartCoroutine(WaitAndDestroyButtons());

            IEnumerator WaitAndDestroyButtons()
            {
                yield return new WaitForSeconds(0.35f);

                module2.SetActive(false);

                if (spawnModule2ButtonsCor != null)
                {
                    StopCoroutine(spawnModule2ButtonsCor);
                    spawnModule2ButtonsCor = null;
                }

                foreach (GameObject obj in spawnedMapButtons)
                {
                    Destroy(obj);
                }

                spawnedMapButtons.Clear();
            }
        }

        switch(view)
        {
            case 0:
                camAnim.SetInteger("view", MAIN_MENU);
                camAnim.SetTrigger("switch");
                break;
            case 1:
                camAnim.SetInteger("view", BOTTOM_1);
                camAnim.SetTrigger("switch");
                module1.SetActive(true);
                module2.SetActive(false);
                module3.SetActive(false);
                credits.SetActive(false);
                ModuleNumber = 1;
                break;
            case 2:
                camAnim.SetInteger("view", BOTTOM_1);
                camAnim.SetTrigger("switch");
                module1.SetActive(false);
                module2.SetActive(true);
                module3.SetActive(false);
                credits.SetActive(false);
                module2Levels.SetActive(true);
                module2Info.SetActive(false);
                camAnim.SetTrigger("switch");
                ModuleNumber = 2;
                LoadModule2();
                break;
            case 3:
                camAnim.SetInteger("view", BOTTOM_1);
                camAnim.SetTrigger("switch");
                module1.SetActive(false);
                module2.SetActive(false);
                module3.SetActive(true);
                credits.SetActive(false);
                break;
            case 4:
                camAnim.SetInteger("view", BOTTOM_1);
                camAnim.SetTrigger("switch");
                module1.SetActive(false);
                module2.SetActive(false);
                module3.SetActive(false);
                credits.SetActive(true);
                ModuleNumber = 3;
                break;
        }
    }

    public void StartLevelModule1(string difficulty) => StartCoroutine(OpenLevelCoroutine((Difficulty)System.Enum.Parse(typeof(Difficulty), difficulty)));

    private IEnumerator OpenLevelCoroutine(Difficulty difficulty)
    {
        MapSerializer serializer = new MapSerializer(MapSerializer.MapsPath + "/" + LevelManager.CurrentManager.RandomMap(difficulty).name + ".xml");
        Map deserializedMap = serializer.Deserialize();

        LevelManager.CurrentManager.SetBackgroundColor(deserializedMap.biomeType);

        camAnim.SetInteger("view", LEVEL);
        camAnim.SetTrigger("switch");

        yield return new WaitForSeconds(0.5f);

        module1.SetActive(false);
        module2.SetActive(false);
        module3.SetActive(false);
        credits.SetActive(false);

        camAnim.enabled = false;

        LevelManager.CurrentManager.LoadLevel(deserializedMap);
    }

    private void LoadModule2()
    {
        spawnModule2ButtonsCor = InstantiateButtons();
        StartCoroutine(spawnModule2ButtonsCor);
    }

    IEnumerator InstantiateButtons()
    {
        yield return new WaitForSeconds(0.4f);
        int number = 1;

        for (int i = 0; i < 4; i++)
        {
            Vector3 pos = new Vector3(-6.4f, 8.8f - (3.3f * i), 0f);

            for (int j = 0; j < 5; j++)
            {
                GameObject obj = Instantiate(module2MapButton);
                spawnedMapButtons.Add(obj);
                obj.transform.SetParent(module2Levels.transform);
                obj.transform.localPosition = pos;

                obj.name = "PreLevel" + number.ToString();

                RankingManager.Record? bestResult = RankingManager.GetTheBestRecord("PreLevel" + number.ToString());

                if (bestResult != null)
                {
                    obj.transform.GetChild(0).GetComponent<TMPro.TextMeshPro>().text += $" <size=\"1\"><b>{((RankingManager.Record)bestResult).points.ToString()}</b></size>";
                }
                else
                {
                    obj.transform.GetChild(0).GetComponent<TMPro.TextMeshPro>().text += " <size=\"1\"><b>0</b></size>";
                }

                Button3D btn = obj.GetComponent<Button3D>();

                btn.OnClick.AddListener((sender) =>
                {
                    MapSerializer serializer = new MapSerializer(MapSerializer.MapsPath + "/" + sender.name + ".xml");
                    Map deserializedMap = serializer.Deserialize();

                    module2Levels.SetActive(false);
                    module2Info.SetActive(true);

                    currentLevelModule2 = sender.name;

                    RankingManager.Record[] records = RankingManager.GetRecords(currentLevelModule2);
                    TMPro.TextMeshPro text = module2Info.transform.GetChild(2).GetComponent<TMPro.TextMeshPro>();
                    text.text = "";

                    foreach (RankingManager.Record r in records)
                    {
                        text.text += $"Points: <b>{r.points.ToString()} | </b>Count of moves: <b>{r.moves.ToString()}</b> | Date: <b>{r.date.ToShortDateString()} {r.date.ToShortTimeString()}</b>\n";
                    }

                    Button3D playBtn = module2Info.transform.GetChild(4).GetComponent<Button3D>();
                    Button3D playSavedBtn = module2Info.transform.GetChild(5).GetComponent<Button3D>();

                    playSavedBtn.isClickable = SaveLoadManager.SaveExists(deserializedMap);

                    playSavedBtn.OnClick.RemoveAllListeners();

                    if(playSavedBtn.isClickable)
                    {
                        playSavedBtn.OnClick.AddListener(s =>
                        {
                            Map progressedMap = SaveLoadManager.LoadLevelProgress(deserializedMap, out int movesCount);

                            StartCoroutine(Coroutine());

                            IEnumerator Coroutine()
                            {
                                camAnim.SetInteger("view", LEVEL);
                                camAnim.SetTrigger("switch");

                                yield return new WaitForSeconds(0.5f);

                                if (spawnModule2ButtonsCor != null)
                                {
                                    StopCoroutine(spawnModule2ButtonsCor);
                                    spawnModule2ButtonsCor = null;
                                }

                                foreach (GameObject o in spawnedMapButtons)
                                {
                                    Destroy(o);
                                }

                                spawnedMapButtons.Clear();

                                module1.SetActive(false);
                                module2.SetActive(false);
                                module3.SetActive(false);
                                credits.SetActive(false);

                                camAnim.enabled = false;

                                LevelManager.CurrentManager.SetBackgroundColor(progressedMap.biomeType);
                                LevelManager.CurrentManager.LoadLevel(progressedMap, movesCount);
                            }
                        });
                    }

                    playBtn.OnClick.RemoveAllListeners();
                    playBtn.OnClick.AddListener(s =>
                    {
                        SaveLoadManager.ClearSave(deserializedMap);

                        StartCoroutine(Coroutine());

                        IEnumerator Coroutine()
                        {
                            camAnim.SetInteger("view", LEVEL);
                            camAnim.SetTrigger("switch");

                            yield return new WaitForSeconds(0.5f);

                            if (spawnModule2ButtonsCor != null)
                            {
                                StopCoroutine(spawnModule2ButtonsCor);
                                spawnModule2ButtonsCor = null;
                            }

                            foreach (GameObject o in spawnedMapButtons)
                            {
                                Destroy(o);
                            }

                            spawnedMapButtons.Clear();

                            module1.SetActive(false);
                            module2.SetActive(false);
                            module3.SetActive(false);
                            credits.SetActive(false);

                            camAnim.enabled = false;

                            LevelManager.CurrentManager.SetBackgroundColor(deserializedMap.biomeType);
                            LevelManager.CurrentManager.LoadLevel(deserializedMap);
                        }
                    });
                });
                
                number++;
                pos += new Vector3(5.3f, 0f, 0f);
                yield return new WaitForSeconds(0.075f);
            }
        }

        spawnModule2ButtonsCor = null;
    }

    public void NewMap()
    {
        StartCoroutine(NewMapCoroutine());

        IEnumerator NewMapCoroutine()
        {
            camAnim.SetInteger("view", LEVEL);
            camAnim.SetTrigger("switch");

            yield return new WaitForSeconds(0.5f);

            module1.SetActive(false);
            module2.SetActive(false);
            module3.SetActive(false);
            credits.SetActive(false);

            camAnim.enabled = false;
            mapEditor.InitializeEditor(new Vector2Int(12, 8));
        }
    }

    /// <summary>
    /// In build closes game, in editor exits play mode.
    /// </summary>
    public void ExitGame()
    {
        if(Application.isEditor)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
        else
        {
            Application.Quit();
        }
    }

    public void CloseModule2LevelInfo()
    {
        module2Levels.SetActive(true);
        module2Info.SetActive(false);
    }
}
