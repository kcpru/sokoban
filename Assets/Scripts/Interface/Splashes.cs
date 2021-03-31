using UnityEngine;
using TMPro;

public class Splashes : MonoBehaviour
{
    [SerializeField] private TextAsset splashesSource;

    private TextMeshPro text;
    private string[] splashes;

    private void Start()
    {
        text = GetComponent<TextMeshPro>();
        splashes = splashesSource.text.Split('\n');
        RandomSplash();
    }

    public void RandomSplash() => text.text = splashes[Random.Range(0, splashes.Length)];
}
