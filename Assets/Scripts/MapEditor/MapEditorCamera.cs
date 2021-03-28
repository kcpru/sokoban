using UnityEngine;

public class MapEditorCamera : MonoBehaviour
{
    [Header("Camera rotation")]
    [SerializeField] private float speedHorizontal = 2f;
    [SerializeField] private float speedVertical = 2f;
    private float yaw = 0f;
    private float pitch = 0f;

    [Header("Camera position")]
    [SerializeField] private float moveSpeed = 4f;
    private float xCamPos = 0f, zCamPos = 0f;
    private float xCamMin, xCamMax, zCamMin, zCamMax;

    public bool IsActivated { get; private set; } = false;

    public void ActivateController(Vector2Int gridSize)
    {
        yaw = transform.eulerAngles.y;
        pitch = transform.eulerAngles.x;
        xCamPos = transform.position.x;
        zCamPos = transform.position.z;

        xCamMin = transform.position.x - (gridSize.x * 0.6f);
        xCamMax = transform.position.x + (gridSize.x * 0.6f);
        zCamMin = transform.position.z;
        zCamMax = transform.position.z + (gridSize.y * 0.85f);

        IsActivated = true;
    }

    public void DeactivateController() => IsActivated = false;
    
    private void Update()
    {
        if(IsActivated)
        {
            if (Input.GetMouseButton(1))
            {
                yaw += speedHorizontal * Input.GetAxis("Mouse X");
                yaw = Mathf.Clamp(yaw, -15f, 15f);

                pitch += speedVertical * -Input.GetAxis("Mouse Y");
                pitch = Mathf.Clamp(pitch, 45f, 75f);

                Vector3 rot = new Vector3(pitch, yaw, 0f);

                transform.eulerAngles = rot;
            }

            if (Input.GetMouseButton(2))
            {
                xCamPos += moveSpeed * -Input.GetAxis("Mouse X");
                xCamPos = Mathf.Clamp(xCamPos, xCamMin, xCamMax);

                zCamPos += moveSpeed * -Input.GetAxis("Mouse Y");
                zCamPos = Mathf.Clamp(zCamPos, zCamMin, zCamMax);

                transform.position = new Vector3(xCamPos, transform.position.y, zCamPos);
            }
        }
    }
}
