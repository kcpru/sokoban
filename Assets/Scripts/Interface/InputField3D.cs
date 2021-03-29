using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class InputField3D : MonoBehaviour
{
    public string value;
    public int maxLength = 32;

    public UnityEvent<MonoBehaviour> OnValueChanged = new UnityEvent<MonoBehaviour>();

    private TextMeshPro text;
    private Renderer render;

    private bool isFocus = false;
    private bool isCursor = false;
    private bool isBackspace = false;

    private void Start()
    {
        text = transform.GetChild(0).GetComponent<TextMeshPro>();
        render = GetComponent<Renderer>();
        isFocus = false;
        text.text = value;
    }

    private void Update()
    {
        if (isFocus && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)) && !isCursor)
        {
            isFocus = false;
            render.material.color = Color.white;
        }

        if (!isFocus && Input.GetMouseButtonDown(0) && isCursor)
        {
            isFocus = true;
            render.material.color = Color.gray;
        }

        isBackspace = Input.GetKey(KeyCode.Backspace);

        if (isFocus)
        {
            if(Input.GetKeyDown(KeyCode.Backspace))
            {
                if(value.Length > 0)
                {
                    value = value.Remove(value.Length - 1, 1);
                    text.text = value;
                    OnValueChanged.Invoke(this);
                }
            }
            else
            {
                if (value.Length < maxLength && !isBackspace)
                {
                    value += Input.inputString;
                    text.text = value;
                    OnValueChanged.Invoke(this);
                }
            }           
        }
    }

    public void SetValue(string value)
    {
        this.value = value;
        text.text = value;
    }

    private void OnMouseEnter() => isCursor = true;

    private void OnMouseExit() => isCursor = false;
}
