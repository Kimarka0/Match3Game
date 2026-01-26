using System.Collections.Generic;
using UnityEngine;

public class MatchChecker
{
    private readonly Cell[,] _cells;
    private readonly int _width;
    private readonly int _height;

    public MatchChecker(Cell[,] cells, int width, int height)
    {
        _cells = cells;
        _width = width;
        _height = height;
    }
    public bool HasMatchAt(int x, int y)
    {
        return HasHorizontalMatch(x, y) || HasVerticalMatch(x, y);
    }
    public List<Cell> FindAllMatch()
    {
        List<Cell> mathedCells = new List<Cell>();
        for(int x = 0; x < _width; x++)
        {
            for(int y = 0; y < _height; y++)
            {
                if (HasMatchAt(x, y))
                {
                    mathedCells.Add(_cells[x,y]);
                }
            }
        }
        return mathedCells;
    }
    private bool HasHorizontalMatch(int x, int y)
    {
        if(x > _width - 3) return false;

        TileType type = _cells[x,y].Tile.Type;

        return type == _cells[x + 1, y].Tile.Type  && type ==  _cells[x + 2, y].Tile.Type;
    }

     private bool HasVerticalMatch(int x, int y)
    {
        if(y > _height - 3) return false;
        
        TileType type = _cells[x, y].Tile.Type;

        return type == _cells[x, y + 1].Tile.Type && type == _cells[x, y + 2].Tile.Type;
    }

}
