using UnityEngine;
using TMPro;
using System.Collections;

public class MapEditorMsg : MonoBehaviour
{
    private TextMeshPro textObj;
    private IEnumerator coroutine;

    private void Start() => textObj = GetComponent<TextMeshPro>();

    public void SetText(string text, float time, Color color)
    {
        if(coroutine != null)
        {
            StopCoroutine(coroutine);
        }

        textObj.color = color;
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
