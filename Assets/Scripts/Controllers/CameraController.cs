using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10f;
    private Vector3 targetPosition;

    private void Awake() => GameManager.camera = this;

    private void Start()
    {
        targetPosition = transform.position;
    }

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
    }

    public void SmoothlyChangePosition(Vector3 pos) => targetPosition = pos;

    public void SmoothlyChangePosition(Vector3 pos, float speed)
    {
        moveSpeed = speed;
        targetPosition = pos;
    }
}
