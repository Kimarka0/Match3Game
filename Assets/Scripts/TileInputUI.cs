using UnityEngine;
using UnityEngine.EventSystems;

public class TileInputUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private Board board;
    private Tile tile;
    private Vector2 startPointerPos;

    public void Init(Board board, Tile tile)
    {
        this.board = board;
        this.tile = tile;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        startPointerPos = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Vector2 delta = eventData.position - startPointerPos;

        if(delta.magnitude < 50f) return;

        Vector2Int direction;

        if(Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            direction = delta.x > 0 ? Vector2Int.right : Vector2Int.left;
        }
        else
        {
            direction = delta.y > 0 ? Vector2Int.up : Vector2Int.down;
        }
    board.TrySwapWithDirection(tile, direction);
    }

}
