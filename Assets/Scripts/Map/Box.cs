using UnityEngine;

public class Box : MonoBehaviour
{
    [SerializeField] private Material offMaterial;
    [SerializeField] private Material onMaterial;

    public bool IsOnTarget { get; private set; }

    private Renderer render;

    private void Start() => render = GetComponent<Renderer>();

    public void EnterTarget()
    {
        if (render == null)
            Start();

        IsOnTarget = true;
        render.material = onMaterial;
    }

    public void ExitTarget()
    {
        if (render == null)
            Start();

        IsOnTarget = false;
        render.material = offMaterial;
    }
}
