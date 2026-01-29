using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Sprite[] sprites;

    public TileType Type { get; private set; }

    public void Init(TileType type, Board board)
    {
        Type = type;
        image.sprite = sprites[(int)type];

        TileInputUI input = GetComponent<TileInputUI>();
        if (input != null)
            input.Init(board, this);
    }
}
