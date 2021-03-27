using UnityEngine;
using UnityEngine.Events;

public class Button3D : MonoBehaviour
{
    public UnityEvent<MonoBehaviour> OnClick = new UnityEvent<MonoBehaviour>();

    private bool isMouseOver = false;
    private bool click = false;

    public void Print(string m)
    {
        print(m);
    }

    private void OnMouseEnter() => isMouseOver = true;

    private void OnMouseExit() => isMouseOver = false;

    private void OnMouseDown() => click = true;

    private void OnMouseUp()
    {
        if(click && isMouseOver)
        {
            OnClick.Invoke(this);
        }
    }
}
