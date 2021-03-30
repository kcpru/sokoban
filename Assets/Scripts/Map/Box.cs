using UnityEngine;

public class Box : MonoBehaviour
{
    public float speed = 4f;
    public bool IsOnTarget { get; private set; }

    private Transform glowingBox;

    private Vector3 startScale;
    private Vector3 targetScale;

    private void Start()
    {
        if (glowingBox == null)
        {
            glowingBox = transform.GetChild(1);
            startScale = glowingBox.localScale;
            targetScale = glowingBox.localScale;
        }
    }

    private void Update() => glowingBox.localScale = Vector3.Lerp(glowingBox.localScale, targetScale, Time.deltaTime * speed);

    public void EnterTarget()
    {
        if (glowingBox == null)
            Start();

        IsOnTarget = true;
        targetScale = Vector3.one * 0.8f;
    }

    public void ExitTarget()
    {
        if (glowingBox == null)
            Start();

        IsOnTarget = false;
        targetScale = startScale;
    }
}
