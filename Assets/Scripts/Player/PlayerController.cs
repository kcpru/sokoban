using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            MovePlayer(-Vector2Int.up);
        }

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            MovePlayer(Vector2Int.up);
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            MovePlayer(Vector2Int.right);
        }

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            MovePlayer(-Vector2Int.right);
        }
    }

    public void MovePlayer(Vector2Int direction)
    {
        Vector2Int pos = 
            new Vector2Int(MapManager.CurrentMapManager.PlayerPosition.x + direction.x, MapManager.CurrentMapManager.PlayerPosition.y + direction.y);
        MapManager.CurrentMapManager.MovePlayer(pos, direction);
    }
}
