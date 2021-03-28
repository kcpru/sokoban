using UnityEngine;
using UnityEngine.Events;

public class GridField : MonoBehaviour
{
    public UnityEvent<Vector3> OnClick = new UnityEvent<Vector3>();

    private bool click = false;

    private Renderer render;

    private void Start() => render = GetComponent<Renderer>();

    private void OnMouseEnter()
    {
        render.material.color = new Color(1f, 1f, 1f, 125f / 255f);

        if(Input.GetMouseButton(0))
        {
            OnClick.Invoke(transform.position);
        }
    }

    private void OnMouseExit()
    {
        render.material.color = new Color(1f, 1f, 1f, 50f / 255f);
    }

    private void OnMouseDown() => OnClick.Invoke(transform.position);
}
