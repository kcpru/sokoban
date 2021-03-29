using UnityEngine;
using TMPro;
using System.Collections;

public class MapEditorErrorMsg : MonoBehaviour
{
    private TextMeshPro textObj;
    private IEnumerator coroutine;

    private void Start() => textObj = GetComponent<TextMeshPro>();

    public void SetText(string text, float time)
    {
        if(coroutine != null)
        {
            StopCoroutine(coroutine);
        }

        coroutine = DisplayMessage(text, time);
        StartCoroutine(coroutine);
    }

    private IEnumerator DisplayMessage(string text, float time)
    {
        textObj.text = text;
        yield return new WaitForSeconds(time);
        textObj.text = "";
        coroutine = null;
    }
}
