using UnityEngine;
using UnityEngine.Events;

public class Button3D : MonoBehaviour
{
    [Header("Event settings")]
    public bool isClickable = true;
    public UnityEvent<MonoBehaviour> OnClick = new UnityEvent<MonoBehaviour>();

    [Header("Animation settings")]
    public float transitionSpeed = 10f;
    public float scaleMultiplier = 1.1f;

    private bool isMouseOver = false;
    private bool click = false;

    private Vector3 startScale;
    private Vector3 targetScale;

    private void Start()
    {
        startScale = transform.localScale;
        targetScale = startScale;
    }

    private void OnMouseEnter()
    {
        isMouseOver = true;
        targetScale = isClickable ? startScale * scaleMultiplier : startScale;
    }

    private void OnMouseExit()
    {
        isMouseOver = false;
        targetScale = startScale;
    }

    private void Update() => transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * transitionSpeed);

    private void OnMouseDown() => click = true && isClickable;

    private void OnMouseUp()
    {
        if(click && isMouseOver)
        {
            SoundsManager.Manager.ClickSound.Play();
            OnClick.Invoke(this);
        }
    }
}
