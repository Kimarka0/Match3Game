using UnityEngine;

public class BoardView
{
    private readonly float _cellSize;
    private readonly float _offsetX;
    private readonly float _offsetY;

    public BoardView(int width, int height, float cellSize)
    {
        _cellSize = cellSize;
        _offsetX = (width - 1) * cellSize / 2f;
        _offsetY = (height - 1) * cellSize / 2f;
    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        float worldX = (x * _cellSize) - _offsetX;
        float worldY = (y * _cellSize) - _offsetY;

        return new Vector3(worldX, worldY, 0f);
    }

    public void UpdateTilePosition(Cell cell)
    {
        if (cell.Tile == null) return;

        cell.Tile.transform.position = GetWorldPosition(cell.X, cell.Y);
    }

    public void UpdateTilePositions(Cell firstCell, Cell secondCell)
    {
        UpdateTilePosition(firstCell);
        UpdateTilePosition(secondCell);
    }
}

