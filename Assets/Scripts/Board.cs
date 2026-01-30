
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    [Header("Board Size")]
    [SerializeField] private int width = 8;
    [SerializeField] private int height = 8;
    [SerializeField] private float cellSize = 1.2f;

    [Header("Prefabs & Scene References")]
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private Transform tilesRoot;
    [SerializeField] private TileAnimator tileAnimator;

    // Runtime
    private Cell[,] cells;
    private MatchChecker matchChecker;
    private BoardView boardView;
    private bool isBusy;

    public bool IsBusy => isBusy;
    public Action OnMoveMade; // успешный ход (свап, который дал матч)
    public Action<Dictionary<TileType, int>> OnTilesCleared; // сколько тайлов какого типа было удалено


    private void Start()
    {
        CreateBoard();
    }

    // ============================================================
    // Public API
    // ============================================================

    public void TrySwapWithDirection(Tile tile, Vector2Int direction)
    {
        if (isBusy) return;
        if (tile == null) return;

        Cell firstCell = FindCellByTile(tile);
        if (firstCell == null) return;

        int secondX = firstCell.X + direction.x;
        int secondY = firstCell.Y + direction.y;

        if (!IsInsideBoard(secondX, secondY)) return;

        Cell secondCell = cells[secondX, secondY];
        if (!AreNeighbours(firstCell, secondCell)) return;

        StartCoroutine(TrySwapRoutine(firstCell, secondCell));
    }

    public void RebuildBoard()
{
    if (isBusy) return;
    StartCoroutine(RebuildBoardRoutine());
}

public void ShuffleBoard()
{
    if (isBusy) return;
    StartCoroutine(ShuffleBoardRoutine());
}


    // ============================================================
    // Board Setup
    // ============================================================

    private void CreateBoard()
    {
        cells = new Cell[width, height];
        boardView = new BoardView(width, height, cellSize);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Cell cell = new Cell(x, y);
                cells[x, y] = cell;

                CreateTileForCell(cell);
            }
        }

        matchChecker = new MatchChecker(cells, width, height);
    }

    private void CreateTileForCell(Cell cell)
    {
        Tile tile = Instantiate(tilePrefab, tilesRoot);

        TileType type = GetValidRandomTileType(cell.X, cell.Y);
        tile.Init(type, this);

        cell.Tile = tile;
        boardView.UpdateTilePosition(cell);
    }

    // ============================================================
    // Swap Flow
    // ============================================================

    private IEnumerator TrySwapRoutine(Cell firstCell, Cell secondCell)
    {
        isBusy = true;

        Tile firstTile = firstCell.Tile;
        Tile secondTile = secondCell.Tile;

        if (firstTile == null || secondTile == null)
        {
            isBusy = false;
            yield break;
        }

        // 1) animate swap
        Vector3 firstTarget = boardView.GetWorldPosition(secondCell.X, secondCell.Y);
        Vector3 secondTarget = boardView.GetWorldPosition(firstCell.X, firstCell.Y);

        yield return PlaySwapAndWait(firstTile, firstTarget, secondTile, secondTarget);

        // 2) swap model data
        SwapTilesDataOnly(firstCell, secondCell);

        // 3) match check
        bool hasMatch =
            matchChecker.HasMatchAt(firstCell.X, firstCell.Y) ||
            matchChecker.HasMatchAt(secondCell.X, secondCell.Y);

        if (!hasMatch)
        {
            // 4) animate back
            Vector3 firstBack = boardView.GetWorldPosition(firstCell.X, firstCell.Y);
            Vector3 secondBack = boardView.GetWorldPosition(secondCell.X, secondCell.Y);

            yield return PlaySwapAndWait(firstTile, firstBack, secondTile, secondBack);

            // 5) revert data
            SwapTilesDataOnly(firstCell, secondCell);

            isBusy = false;
            yield break;
        }

        // 6) resolve cascades
        OnMoveMade?.Invoke();
        yield return ResolveBoardRoutine();

        isBusy = false;
    }

    private IEnumerator PlaySwapAndWait(Tile firstTile, Vector3 firstTarget, Tile secondTile, Vector3 secondTarget)
    {
        bool finished = false;
        tileAnimator.PlaySwap(firstTile, firstTarget, secondTile, secondTarget, () => finished = true);
        while (!finished) yield return null;
    }

    // ============================================================
    // Resolve / Matches / Gravity
    // ============================================================

    private IEnumerator ResolveBoardRoutine()
    {
        while (true)
        {
            List<Cell> matchedCells = matchChecker.FindAllMatch();
            if (matchedCells.Count == 0) yield break;

            yield return AnimateAndClearMatchesRoutine(matchedCells);

            ApplyGravity();
            SpawnNewTiles();

            yield return AnimateAllTilesToCellPositionsRoutine();
        }
    }

    private IEnumerator AnimateAndClearMatchesRoutine(List<Cell> matchedCells)
    {
        // Collect tiles to animate delete
        List<Tile> matchedTiles = new List<Tile>(matchedCells.Count);
        for (int i = 0; i < matchedCells.Count; i++)
        {
            Tile tile = matchedCells[i].Tile;
            if (tile != null) matchedTiles.Add(tile);
        }
        Dictionary<TileType, int> clearedByType = new Dictionary<TileType, int>();

        for (int i = 0; i < matchedCells.Count; i++)
        {
            Tile t = matchedCells[i].Tile;
            if (t == null) continue;

            if (!clearedByType.ContainsKey(t.Type))
                clearedByType[t.Type] = 0;

            clearedByType[t.Type]++;
        }

        if (clearedByType.Count > 0)
            OnTilesCleared?.Invoke(clearedByType);


        bool finished = false;
        tileAnimator.PlayDelete(matchedTiles, () => finished = true);
        while (!finished) yield return null;

        // Destroy and clear cells
        for (int i = 0; i < matchedCells.Count; i++)
        {
            Cell cell = matchedCells[i];
            if (cell.Tile == null) continue;

            Destroy(cell.Tile.gameObject);
            cell.Tile = null;
        }
    }

    private void ApplyGravity()
    {
        bool moved;
        do
        {
            moved = false;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height - 1; y++)
                {
                    if (cells[x, y].Tile != null) continue;

                    int yAbove = -1;
                    for (int checkY = y + 1; checkY < height; checkY++)
                    {
                        if (cells[x, checkY].Tile != null)
                        {
                            yAbove = checkY;
                            break;
                        }
                    }

                    if (yAbove == -1) continue;

                    cells[x, y].Tile = cells[x, yAbove].Tile;
                    cells[x, yAbove].Tile = null;

                    moved = true;
                }
            }
        }
        while (moved);
    }

    private void SpawnNewTiles()
    {
        float spawnYOffset = cellSize * 2f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (cells[x, y].Tile != null) continue;

                Tile tile = Instantiate(tilePrefab, tilesRoot);

                TileType type = GetValidRandomTileType(x, y);
                tile.Init(type, this);

                Vector3 topPos = boardView.GetWorldPosition(x, height - 1);
                tile.transform.position = new Vector3(topPos.x, topPos.y + spawnYOffset, 0f);

                cells[x, y].Tile = tile;
            }
        }
    }

    private IEnumerator AnimateAllTilesToCellPositionsRoutine(float duration = 0.18f)
    {
        List<Tile> tiles = new List<Tile>();
        List<Vector3> starts = new List<Vector3>();
        List<Vector3> targets = new List<Vector3>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile tile = cells[x, y].Tile;
                if (tile == null) continue;

                tiles.Add(tile);
                starts.Add(tile.transform.position);
                targets.Add(boardView.GetWorldPosition(x, y));
            }
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            for (int i = 0; i < tiles.Count; i++)
            {
                Tile tile = tiles[i];
                if (tile == null) continue;

                tile.transform.position = Vector3.Lerp(starts[i], targets[i], t);
            }

            yield return null;
        }

        for (int i = 0; i < tiles.Count; i++)
        {
            Tile tile = tiles[i];
            if (tile == null) continue;

            tile.transform.position = targets[i];
        }
    }
    // ============================================================
// Rebuild / Shuffle
// ============================================================

private IEnumerator RebuildBoardRoutine()
{
    isBusy = true;

    // остановим любые анимационные корутины свапа/резолва, если вдруг
    StopAllCoroutines();

    // но StopAllCoroutines остановит и эту рутину, поэтому запускаем заново:
    StartCoroutine(RebuildBoardRoutine_Internal());
    yield break;
}

private IEnumerator RebuildBoardRoutine_Internal()
{
    // 1) уничтожаем все тайлы
    DestroyAllTiles();
    yield return null; // дать кадр на обработку Destroy

    // 2) создаём борд заново (как в Start)
    CreateBoard();

    isBusy = false;
    yield break;
}

private IEnumerator ShuffleBoardRoutine()
{
    isBusy = true;

    // если нет борда — просто rebuild
    if (cells == null || matchChecker == null)
    {
        yield return RebuildBoardRoutine_Internal();
        yield break;
    }

    bool ok = TryShuffleWithValidation(40); // 40 попыток перемешивания

    if (!ok)
    {
        // если не получилось сделать валидную доску — пересоздаём
        yield return RebuildBoardRoutine_Internal();
        yield break;
    }

    isBusy = false;
}

// ------------------------------------------------------------
// Shuffle internals
// ------------------------------------------------------------

private bool TryShuffleWithValidation(int attempts)
{
    List<TileType> types = GetAllTileTypes();
    if (types.Count == 0) return false;

    for (int attempt = 0; attempt < attempts; attempt++)
    {
        ShuffleTypes(types);
        ApplyTypes(types);

        // 1) на старте не должно быть готовых матчей
        if (matchChecker.FindAllMatch().Count > 0) continue;

        // 2) должен существовать хотя бы 1 ход
        if (!HasAnyPossibleMove()) continue;

        return true;
    }

    return false;
}

private List<TileType> GetAllTileTypes()
{
    List<TileType> types = new List<TileType>(width * height);

    for (int x = 0; x < width; x++)
    {
        for (int y = 0; y < height; y++)
        {
            Tile tile = cells[x, y].Tile;
            if (tile == null) continue;
            types.Add(tile.Type);
        }
    }

    return types;
}

private void ApplyTypes(List<TileType> types)
{
    int index = 0;

    for (int x = 0; x < width; x++)
    {
        for (int y = 0; y < height; y++)
        {
            Tile tile = cells[x, y].Tile;
            if (tile == null) continue;

            // tile.Init обновит sprite и оставит input рабочим
            tile.Init(types[index], this);
            index++;
        }
    }
}

private void ShuffleTypes(List<TileType> list)
{
    // Fisher–Yates
    for (int i = list.Count - 1; i > 0; i--)
    {
        int j = UnityEngine.Random.Range(0, i + 1);
        TileType temp = list[i];
        list[i] = list[j];
        list[j] = temp;
    }
}

private bool HasAnyPossibleMove()
{
    // проверяем только вправо и вверх (чтобы не дублировать проверки)
    for (int x = 0; x < width; x++)
    {
        for (int y = 0; y < height; y++)
        {
            Cell a = cells[x, y];
            if (a.Tile == null) continue;

            if (x + 1 < width)
            {
                Cell b = cells[x + 1, y];
                if (b.Tile != null && WouldSwapCreateMatch(a, b)) return true;
            }

            if (y + 1 < height)
            {
                Cell b = cells[x, y + 1];
                if (b.Tile != null && WouldSwapCreateMatch(a, b)) return true;
            }
        }
    }

    return false;
}

private bool WouldSwapCreateMatch(Cell firstCell, Cell secondCell)
{
    SwapTilesDataOnly(firstCell, secondCell);

    bool hasMatch =
        matchChecker.HasMatchAt(firstCell.X, firstCell.Y) ||
        matchChecker.HasMatchAt(secondCell.X, secondCell.Y);

    SwapTilesDataOnly(firstCell, secondCell);

    return hasMatch;
}

// ------------------------------------------------------------
// Rebuild internals
// ------------------------------------------------------------

private void DestroyAllTiles()
{
    if (cells == null) return;

    for (int x = 0; x < width; x++)
    {
        for (int y = 0; y < height; y++)
        {
            Tile tile = cells[x, y].Tile;
            if (tile == null) continue;

            Destroy(tile.gameObject);
            cells[x, y].Tile = null;
        }
    }
}


    // ============================================================
    // Helpers
    // ============================================================

    private bool IsInsideBoard(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    private bool AreNeighbours(Cell firstCell, Cell secondCell)
    {
        if (firstCell.X == secondCell.X && Mathf.Abs(secondCell.Y - firstCell.Y) == 1) return true;
        if (firstCell.Y == secondCell.Y && Mathf.Abs(secondCell.X - firstCell.X) == 1) return true;
        return false;
    }

    private void SwapTilesDataOnly(Cell firstCell, Cell secondCell)
    {
        Tile temp = firstCell.Tile;
        firstCell.Tile = secondCell.Tile;
        secondCell.Tile = temp;
    }

    private Cell FindCellByTile(Tile tile)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (cells[x, y].Tile == tile) return cells[x, y];
            }
        }

        return null;
    }

    private TileType GetValidRandomTileType(int x, int y)
    {
        TileType type;
        do
        {
            type = (TileType)UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(TileType)).Length);
        }
        while (!IsTileTypeValid(x, y, type));

        return type;
    }

    private bool IsTileTypeValid(int x, int y, TileType type)
    {
        if (x >= 2)
        {
            Tile left = cells[x - 1, y].Tile;
            Tile leftFar = cells[x - 2, y].Tile;

            if (left != null && leftFar != null && left.Type == type && leftFar.Type == type)
                return false;
        }

        if (y >= 2)
        {
            Tile bottom = cells[x, y - 1].Tile;
            Tile bottomFar = cells[x, y - 2].Tile;

            if (bottom != null && bottomFar != null && bottom.Type == type && bottomFar.Type == type)
                return false;
        }

        return true;
    }
}
