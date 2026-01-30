using UnityEngine;
using UnityEngine.EventSystems;

public class TileInputUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private const float swipeThreshold = 30f;

    private Board board;
    private Tile tile;

    private Vector2 startPointerPos;
    private bool pressed;

    public void Init(Board board, Tile tile)
    {
        this.board = board;
        this.tile = tile;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (board == null) return;
        if (board.IsBusy) return;

        pressed = true;
        startPointerPos = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!pressed) return;
        pressed = false;

        if (board == null) return;
        if (board.IsBusy) return;

        Vector2 delta = eventData.position - startPointerPos;
        if (delta.magnitude < swipeThreshold) return;

        Vector2Int direction;
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            direction = delta.x > 0 ? Vector2Int.right : Vector2Int.left;
        else
            direction = delta.y > 0 ? Vector2Int.up : Vector2Int.down;

        board.TrySwapWithDirection(tile, direction);
    }
}
