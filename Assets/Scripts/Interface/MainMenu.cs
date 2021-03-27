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

    [Header("Module 2")]
    [SerializeField] private GameObject module2MapButton;
    private List<GameObject> spawnedMapButtons = new List<GameObject>();

    public const int MAIN_MENU = 0;
    public const int BOTTOM_1 = 1;
    public const int LEVEL = 2;
    public const int EDITOR = 3;

    private Transform camTrans;

    private IEnumerator spawnModule2ButtonsCor;

    private void Start()
    {
        camTrans = camAnim.transform;
    }

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
                break;
            case 2:
                camAnim.SetInteger("view", BOTTOM_1);
                camAnim.SetTrigger("switch");
                module1.SetActive(false);
                module2.SetActive(true);
                module3.SetActive(false);
                credits.SetActive(false);
                camAnim.SetTrigger("switch");
                LoadModule2();
                break;
            case 3:
                camAnim.SetInteger("view", BOTTOM_1);
                camAnim.SetTrigger("switch");
                module1.SetActive(false);
                module2.SetActive(true);
                module3.SetActive(false);
                credits.SetActive(false);
                break;
            case 4:
                camAnim.SetInteger("view", BOTTOM_1);
                camAnim.SetTrigger("switch");
                module1.SetActive(false);
                module2.SetActive(false);
                module3.SetActive(false);
                credits.SetActive(true);
                break;
        }
    }

    public void StartLevelModule1(string difficulty) => StartCoroutine(OpenLevelCoroutine((Difficulty)System.Enum.Parse(typeof(Difficulty), difficulty)));

    private IEnumerator OpenLevelCoroutine(Difficulty difficulty)
    {
        MapSerializer serializer = new MapSerializer(MapSerializer.MapsPath + "/" + LevelManager.CurrentManager.RandomMap(difficulty).name + ".xml");
        Map deserializedMap = serializer.Deserialize();

        LevelManager.CurrentManager.SetBackgroundColor(deserializedMap.biomeType);
        print(deserializedMap.name);

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

        for (int i = 0; i < 4; i++)
        {
            Vector3 pos = new Vector3(-6.4f, 8.8f - (3.3f * i), 0f);

            for (int j = 0; j < 5; j++)
            {
                GameObject obj = Instantiate(module2MapButton);
                spawnedMapButtons.Add(obj);
                obj.transform.SetParent(module2.transform);
                obj.transform.localPosition = pos;
                pos += new Vector3(5.3f, 0f, 0f);
                yield return new WaitForSeconds(0.075f);
            }
        }

        spawnModule2ButtonsCor = null;
    }
}
