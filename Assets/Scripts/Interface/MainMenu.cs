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

    public const int MAIN_MENU = 0;
    public const int BOTTOM_1 = 1;
    public const int LEVEL = 2;
    public const int EDITOR = 3;

    private Transform camTrans;

    private void Start()
    {
        camTrans = camAnim.transform;
    }

    public void OpenPage(int view)
    {
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
}
