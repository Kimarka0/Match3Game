using System.Collections.Generic;

public class MatchChecker
{
    private readonly Cell[,] cells;
    private readonly int width;
    private readonly int height;

    public MatchChecker(Cell[,] cells, int width, int height)
    {
        this.cells = cells;
        this.width = width;
        this.height = height;
    }

    public bool HasMatchAt(int x, int y)
    {
        return HasHorizontalMatch(x, y) || HasVerticalMatch(x, y);
    }

    public List<Cell> FindAllMatch()
    {
        List<Cell> matchedCells = new List<Cell>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (HasMatchAt(x, y))
                    matchedCells.Add(cells[x, y]);
            }
        }

        return matchedCells;
    }

    private bool HasHorizontalMatch(int x, int y)
    {
        Tile center = cells[x, y].Tile;
        if (center == null) return false;

        TileType type = center.Type;
        int count = 1;

        for (int i = x - 1; i >= 0; i--)
        {
            Tile t = cells[i, y].Tile;
            if (t == null || t.Type != type) break;
            count++;
        }

        for (int i = x + 1; i < width; i++)
        {
            Tile t = cells[i, y].Tile;
            if (t == null || t.Type != type) break;
            count++;
        }

        return count >= 3;
    }

    private bool HasVerticalMatch(int x, int y)
    {
        Tile center = cells[x, y].Tile;
        if (center == null) return false;

        TileType type = center.Type;
        int count = 1;

        for (int j = y - 1; j >= 0; j--)
        {
            Tile t = cells[x, j].Tile;
            if (t == null || t.Type != type) break;
            count++;
        }

        for (int j = y + 1; j < height; j++)
        {
            Tile t = cells[x, j].Tile;
            if (t == null || t.Type != type) break;
            count++;
        }

        return count >= 3;
    }
}
