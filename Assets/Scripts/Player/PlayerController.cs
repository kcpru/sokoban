using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    [SerializeField] private float moveOffset = 1f;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            MovePlayer(Vector2Int.up);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            MovePlayer(-Vector2Int.up);
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            MovePlayer(Vector2Int.right);
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            MovePlayer(-Vector2Int.right);
        }
    }

    public void MovePlayer(Vector2Int direction)
    {
        print($"player pos = {MapManager.CurrentMapManager.PlayerPosition}");
        print($"dir = {direction}");
        Vector2Int pos = 
            new Vector2Int(MapManager.CurrentMapManager.PlayerPosition.x + direction.x, MapManager.CurrentMapManager.PlayerPosition.y + direction.y);
        MapManager.CurrentMapManager.MovePlayer(pos, direction);
    }
}
