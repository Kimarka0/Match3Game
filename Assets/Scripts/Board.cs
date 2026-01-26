using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Board : MonoBehaviour
{
    [SerializeField] private int width = 8;
    [SerializeField] private int height = 8;
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private float cellSize = 1.2f;
    [SerializeField] private Transform tilesRoot;
    private Cell[,] cells;
    private MatchChecker matchChecker;

    private void Start()
    {
        CreateBoard();
    }

    private void CreateBoard()
    {
        cells = new Cell[width, height];


        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                Cell cell = new Cell(x, y);
                cells[x,y] = cell;

                CreateTile(cell);
                Debug.Log($"Cell created: {x},{y}");
            }
        }
        matchChecker = new MatchChecker(cells, width, height);
    }

    private void CreateTile(Cell cell)
    {
        Tile tile = Instantiate(tilePrefab, tilesRoot);

        float offsetX = (width - 1)  * cellSize / 2f;
        float offsetY = (height -1 ) * cellSize / 2f;
        tile.transform.position = new Vector3((cell.X * cellSize) - offsetX, (cell.Y * cellSize) - offsetY, 0);

        TileType randomType = GetValidRandomTileType(cell.X, cell.Y);
        tile.Init(randomType);

        cell.Tile = tile;
        Debug.Log($"Tile placed at: {cell.X},{cell.Y} type={randomType}");
        
    }

    private Cell GetCell(int x, int y)
    {
        return cells[x,y];
    }

    private TileType GetTileType(int x, int y)
    {
        return cells[x, y].Tile.Type;;
    }

    private TileType GetValidRandomTileType(int x, int y)
    {
        TileType type;

        do
        {
            type = (TileType)Random.Range(0, System.Enum.GetValues(typeof(TileType)).Length);
        }
        while(!IsTileTypeValid(x, y, type));

        return type;
            
    }


private bool IsTileTypeValid(int x,int y, TileType type)
{
      if(x >= 2)
        {
            Tile  leftNeighbor = cells[x - 1, y].Tile;
            Tile leftFarNeighbor = cells[x - 2, y].Tile;

            if(leftNeighbor != null && leftFarNeighbor != null &&
                leftNeighbor.Type == type && leftFarNeighbor.Type == type)
            {
                return false;
            }
        }  

        if(y >= 2)
        {
            Tile bottomNeighbor = cells[x, y - 1].Tile;
            Tile bottomFarNeighbor = cells[x, y - 2].Tile;

            if(bottomNeighbor != null && bottomFarNeighbor != null
                && bottomNeighbor.Type == type && bottomFarNeighbor.Type == type)
            {
                return false;
            }
        }
        return true;
}

 private void DestroyMatch()
{
    List<Cell> cells = new();  
    cells = matchChecker.FindAllMatch();
    for(int i = 0; i < cells.Count; i++)
    {
        cells[i].Tile.TileDestroy();
    }
}

private bool CheckNeighbours(Cell firstCell, Cell secondsCell)
{
   if(firstCell.X == secondsCell.X && Mathf.Abs(secondsCell.Y - firstCell.Y) == 1) return true;
   if(firstCell.Y == secondsCell.Y && Mathf.Abs(secondsCell.X - firstCell.X) == 1) return true;

   return false;
}

}
