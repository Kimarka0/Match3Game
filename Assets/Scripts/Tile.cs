using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Sprite[] sprites;
    public TileType Type {get;  private set; }
    public UnityEvent onTileDestroyed = new UnityEvent();
    private Board board;

    public void Init(TileType type, Board board)
    {
        Type = type;
        this.board = board;
        image.sprite = sprites[(int)type];

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
