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

    public void Init(TileType type)
    {
        Type = type;
        spriteRenderer.sprite = sprites[(int)type];
    }

    public void TileDestroy()
    {
        onTileDestroyed.Invoke();
        Destroy(gameObject);
    }
}
