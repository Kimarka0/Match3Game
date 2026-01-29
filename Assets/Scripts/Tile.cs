using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class Tile : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] sprites;
    public TileType Type {get;  private set; }
    public UnityEvent onTileDestroyed = new UnityEvent();

    public int X {get; private set;}
    public int Y {get; private set;}

    public void SetCoordinates(int x, int y)
    {
        X = x;
        Y = y;
    }

    public void Init(TileType type, Board board)
    {
        Type = type;
        this.board = board;
        spriteRenderer.sprite = sprites[(int)type];

        TileInputUI input = GetComponent<TileInputUI>();
        if(input != null)
        {
            input.Init(board, this);
        }
    }

    public void TileDestroy()
    {
        onTileDestroyed.Invoke();
        Destroy(gameObject);
    }
}
