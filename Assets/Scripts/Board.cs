using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Board : MonoBehaviour
{
    [SerializeField] private int width = 8;
    [SerializeField] private int height = 8;
    [SerializeField] private TileAnimator tileAnimator;
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private float cellSize = 1.2f;
    [SerializeField] private Transform tilesRoot;
    private Cell[,] cells;
    private MatchChecker matchChecker;
    private BoardView boardView;
    private bool isBusy = false;

    private void Start()
    {
        CreateBoard();

        Cell firstCell = cells[0,0];
        Cell secondCell = cells[0,1];

        Vector3 firstTarget = boardView.GetWorldPosition(secondCell.X, secondCell.Y);
        Vector3 secondTarget = boardView.GetWorldPosition(firstCell.X, firstCell.Y);

        SwapTiles(firstCell, secondCell);

        tileAnimator.PlaySwap(firstCell.Tile, firstTarget, secondCell.Tile, secondTarget, 
        onComplete: () =>
        {
            bool hasMatch = matchChecker.HasMatchAt(firstCell.X, firstCell.Y) || matchChecker.HasMatchAt(secondCell.X, secondCell.Y);

            if (hasMatch)
            {
                DestroyMatch();
                return;
            }
            
            SwapTiles(firstCell,secondCell);

            Vector3 firstTargetBack = boardView.GetWorldPosition(secondCell.X, secondCell.Y);
            Vector3 secondTargetBack = boardView.GetWorldPosition(firstCell.X, firstCell.Y);

            tileAnimator.PlaySwap(firstCell.Tile, firstTargetBack, secondCell.Tile, secondTargetBack);
        });

    }

    private void CreateBoard()
    {
        cells = new Cell[width, height];

        boardView = new(width, height, cellSize);

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

        TileType randomType = GetValidRandomTileType(cell.X, cell.Y);
        tile.Init(randomType);

        cell.Tile = tile;
        boardView.UpdateTilePosition(cell);
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


private bool CheckNeighbours(Cell firstCell, Cell secondsCell)
{
   if(firstCell.X == secondsCell.X && Mathf.Abs(secondsCell.Y - firstCell.Y) == 1) return true;
   if(firstCell.Y == secondsCell.Y && Mathf.Abs(secondsCell.X - firstCell.X) == 1) return true;

   return false;
}

private void SwapTiles(Cell a, Cell b)
    {
        Tile temp = a.Tile;
        a.Tile = b.Tile;
        b.Tile = temp;

        boardView.UpdateTilePositions(a, b);
    }

    private bool TrySwapCells(Cell a, Cell b)
    {
        if (!CheckNeighbours(a, b))
            return false;

        SwapTiles(a, b);

        bool hasMatch =
            matchChecker.HasMatchAt(a.X, a.Y) ||
            matchChecker.HasMatchAt(b.X, b.Y);

        if (hasMatch)
            return true;

        SwapTiles(a, b);
        return false;
    }
private void DestroyMatch()
{
    List<Cell> matchedCells = matchChecker.FindAllMatch();
    List<Tile> matchedTiles = new();
    for(int i = 0; i < matchedCells.Count; i++)
    {
        Cell matchedCell = matchedCells[i];
        if(matchedCell.Tile == null) continue;
        matchedTiles.Add(matchedCell.Tile);
    }

    if(matchedTiles.Count == 0) return;

    if(isBusy) return;

    isBusy = true;

    tileAnimator.PlayDelete(matchedTiles, () =>
    {
        for(int i = 0;i < matchedCells.Count; i++)
        {
            Cell cell = matchedCells[i];
            if(cell.Tile == null) continue;
            Destroy(cell.Tile.gameObject);
            cell.Tile = null;
        }

        isBusy = false;
    });

    
}

}
