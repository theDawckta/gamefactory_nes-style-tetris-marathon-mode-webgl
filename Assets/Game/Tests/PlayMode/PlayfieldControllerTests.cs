using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayfieldControllerTests
{
    private GameObject _go;

    [TearDown]
    public void TearDown()
    {
        if (_go != null)
            UnityEngine.Object.Destroy(_go);
    }

    private PlayfieldController CreateController()
    {
        _go = new GameObject();
        return _go.AddComponent<PlayfieldController>();
    }

    // Helper: use reflection to read the private _grid field
    private TetrominoType?[,] GetGrid(PlayfieldController pf)
    {
        return (TetrominoType?[,])typeof(PlayfieldController)
            .GetField("_grid", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(pf);
    }

    // Helper: fill every cell of the grid with I pieces
    private void FillGrid(PlayfieldController pf)
    {
        var grid = GetGrid(pf);
        for (int x = 0; x < PlayfieldController.GridWidth; x++)
            for (int y = 0; y < PlayfieldController.GridHeight; y++)
                grid[x, y] = TetrominoType.I;
    }

    // Helper: fill a single row
    private void FillRow(PlayfieldController pf, int row)
    {
        var grid = GetGrid(pf);
        for (int x = 0; x < PlayfieldController.GridWidth; x++)
            grid[x, row] = TetrominoType.I;
    }

    // Helper: call private LockPiece via reflection
    private void InvokeLockPiece(PlayfieldController pf)
    {
        typeof(PlayfieldController)
            .GetMethod("LockPiece", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(pf, null);
    }

    // Helper: call private SpawnPiece via reflection
    private void InvokeSpawnPiece(PlayfieldController pf)
    {
        typeof(PlayfieldController)
            .GetMethod("SpawnPiece", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(pf, null);
    }

    // Helper: set CurrentPiecePosition via non-public setter
    private void SetPiecePosition(PlayfieldController pf, Vector2Int pos)
    {
        typeof(PlayfieldController)
            .GetProperty("CurrentPiecePosition", BindingFlags.Public | BindingFlags.Instance)
            .GetSetMethod(nonPublic: true)
            .Invoke(pf, new object[] { pos });
    }

    [UnityTest]
    public IEnumerator StartGame_SpawnsPieceAtColumn3Row18()
    {
        var pf = CreateController();
        yield return null;
        pf.StartGame();
        Assert.AreEqual(3, pf.CurrentPiecePosition.x);
        Assert.AreEqual(18, pf.CurrentPiecePosition.y);
    }

    [UnityTest]
    public IEnumerator StartGame_SetsScoreLinesLevelToZero()
    {
        var pf = CreateController();
        yield return null;
        pf.StartGame();
        Assert.AreEqual(0, pf.CurrentScore);
        Assert.AreEqual(0, pf.CurrentLines);
        Assert.AreEqual(0, pf.CurrentLevel);
    }

    [UnityTest]
    public IEnumerator StartGame_ExposesNextPieceType()
    {
        var pf = CreateController();
        yield return null;
        pf.StartGame();
        // NextPieceType is drawn from the bag; just verify it is a valid enum value
        Assert.IsTrue(Enum.IsDefined(typeof(TetrominoType), pf.NextPieceType));
    }

    [UnityTest]
    public IEnumerator GetCell_ReturnsNullForEmptyCell()
    {
        var pf = CreateController();
        yield return null;
        pf.StartGame();
        // Bottom-left corner is always empty at start
        Assert.IsNull(pf.GetCell(0, 0));
    }

    [UnityTest]
    public IEnumerator GetCellColor_ReturnsClearForEmptyCell()
    {
        var pf = CreateController();
        yield return null;
        pf.StartGame();
        Assert.AreEqual(Color.clear, pf.GetCellColor(0, 0));
    }

    [UnityTest]
    public IEnumerator GetCell_ReturnsNullForOutOfBounds()
    {
        var pf = CreateController();
        yield return null;
        pf.StartGame();
        Assert.IsNull(pf.GetCell(-1, 0));
        Assert.IsNull(pf.GetCell(10, 0));
        Assert.IsNull(pf.GetCell(0, -1));
        Assert.IsNull(pf.GetCell(0, 20));
    }

    [UnityTest]
    public IEnumerator GetCellColor_ReturnsCorrectColor_AfterLocking()
    {
        var pf = CreateController();
        yield return null;
        pf.StartGame();

        // Move piece to row 0 so it won't fill row 18 and cause spawn collision
        // Instead, just lock at current position and check a locked cell is non-clear
        // Lock the active piece in place via reflection
        var pos = pf.CurrentPiecePosition;
        var type = pf.CurrentPieceType;
        InvokeLockPiece(pf);

        // One of the cells the piece occupied should now show a color
        var cells = TetrominoData.GetCells(type, 0);
        var firstCell = cells[0];
        int cx = pos.x + firstCell.x;
        int cy = pos.y + firstCell.y;
        if (cx >= 0 && cx < PlayfieldController.GridWidth && cy >= 0 && cy < PlayfieldController.GridHeight)
            Assert.AreNotEqual(Color.clear, pf.GetCellColor(cx, cy));
    }

    [UnityTest]
    public IEnumerator StopGame_HaltsGameplay()
    {
        var pf = CreateController();
        yield return null;
        pf.StartGame();
        var posAfterStart = pf.CurrentPiecePosition;
        pf.StopGame();
        yield return null; // one Update frame passes but game is stopped
        // Position should not have changed from natural gravity (gravity at level 0 = 0.8s)
        Assert.AreEqual(posAfterStart, pf.CurrentPiecePosition);
    }

    [UnityTest]
    public IEnumerator OnGameOver_FiresWhenSpawnCollides()
    {
        var pf = CreateController();
        yield return null;
        pf.StartGame();

        bool gameOverFired = false;
        pf.OnGameOver += () => gameOverFired = true;

        // Lock the first piece at its spawn row (3,18). All pieces have a cell at (0,0)
        // relative to pivot, so after locking, (3,18) is occupied. The next spawn at (3,18)
        // will collide with that locked cell and fire OnGameOver. No line is fully cleared
        // (only a few cells in row 18), so ClearFullLines leaves them in place.
        InvokeLockPiece(pf);
        yield return null;

        Assert.IsTrue(gameOverFired);
    }

    [UnityTest]
    public IEnumerator OnPieceLocked_FiredWhenPieceLocks()
    {
        var pf = CreateController();
        yield return null;
        pf.StartGame();

        bool pieceLocked = false;
        pf.OnPieceLocked += () => pieceLocked = true;

        InvokeLockPiece(pf);
        yield return null;

        Assert.IsTrue(pieceLocked);
    }

    [UnityTest]
    public IEnumerator OnScoreChanged_FiredWhenScoreChanges()
    {
        var pf = CreateController();
        yield return null;
        pf.StartGame();

        bool scoreChanged = false;
        pf.OnScoreChanged += _ => scoreChanged = true;

        // Fill row 0 and place the piece so it will clear a line
        // Simpler: fill rows 0..8 completely (9 rows), leave row 9 empty except one col.
        // Lock a piece at row 0 by just filling 9 rows and having a full clear.
        // Even simpler: fill rows 0-8 and lock piece at row 0; row 0 won't be full yet.
        // Simplest: fill one full row and lock piece (which clears it → awards points).
        FillRow(pf, 0);

        // Move current piece to row 0 via reflection on position
        SetPiecePosition(pf, new Vector2Int(3, 0));

        InvokeLockPiece(pf);
        yield return null;

        // Row 0 was full + piece cells land there -- at minimum 1 line clear = 40 pts
        Assert.IsTrue(scoreChanged);
        Assert.Greater(pf.CurrentScore, 0);
    }

    [UnityTest]
    public IEnumerator Scoring_SingleLineClear_Awards40xLevel1()
    {
        var pf = CreateController();
        yield return null;
        pf.StartGame();

        // Fill row 0 leaving one gap at column 0
        var grid = GetGrid(pf);
        for (int x = 1; x < PlayfieldController.GridWidth; x++)
            grid[x, 0] = TetrominoType.I;

        // Place a vertical I piece so its bottom cell fills column 0 row 0
        // I rotation 1: cells are (0,-2),(0,-1),(0,0),(0,1) relative to pivot
        // Place at position (0, 1) so cells land at (0,-1),(0,0),(0,1),(0,2)
        // Bottom cell at (0,-1) is out of bounds; doesn't work cleanly.
        // Instead use a simpler fill: set all 10 columns of row 0 directly
        grid[0, 0] = TetrominoType.I;

        // Force the current piece to lock at a safe position (e.g. row 5)
        SetPiecePosition(pf, new Vector2Int(3, 5));

        // Level is 0 so multiplier = 1; single line = 40*1 = 40
        InvokeLockPiece(pf);
        yield return null;

        Assert.AreEqual(40, pf.CurrentScore);
    }

    [UnityTest]
    public IEnumerator Scoring_Tetris_Awards1200xMultiplier()
    {
        var pf = CreateController();
        yield return null;
        pf.StartGame();

        // Fill rows 0-3 completely
        var grid = GetGrid(pf);
        for (int row = 0; row <= 3; row++)
            for (int x = 0; x < PlayfieldController.GridWidth; x++)
                grid[x, row] = TetrominoType.I;

        // Force piece to lock high (row 10) so it doesn't interfere with the filled area
        SetPiecePosition(pf, new Vector2Int(3, 10));

        InvokeLockPiece(pf);
        yield return null;

        // 4 lines cleared at level 0 = 1200 * 1
        Assert.AreEqual(1200, pf.CurrentScore);
    }

    [UnityTest]
    public IEnumerator LevelIncrement_Every10Lines()
    {
        var pf = CreateController();
        yield return null;
        pf.StartGame();

        int levelChangedTo = -1;
        pf.OnLevelChanged += lvl => levelChangedTo = lvl;

        // Clear 10 lines by filling 10 full rows and locking 10 times (1 line per lock)
        for (int i = 0; i < 10; i++)
        {
            // Fill row 0 completely
            var grid = GetGrid(pf);
            for (int x = 0; x < PlayfieldController.GridWidth; x++)
                grid[x, 0] = TetrominoType.I;

            // Place piece at row 5 so it doesn't clog row 0
            SetPiecePosition(pf, new Vector2Int(3, 5));

            InvokeLockPiece(pf);
            yield return null;
        }

        Assert.AreEqual(10, pf.CurrentLines);
        Assert.AreEqual(1, pf.CurrentLevel);
        Assert.AreEqual(1, levelChangedTo);
    }

    [UnityTest]
    public IEnumerator LinesChanged_EventFired_WhenLinesCleared()
    {
        var pf = CreateController();
        yield return null;
        pf.StartGame();

        int linesReported = -1;
        pf.OnLinesChanged += l => linesReported = l;

        // Fill row 0 completely
        var grid = GetGrid(pf);
        for (int x = 0; x < PlayfieldController.GridWidth; x++)
            grid[x, 0] = TetrominoType.I;

        SetPiecePosition(pf, new Vector2Int(3, 5));

        InvokeLockPiece(pf);
        yield return null;

        Assert.AreEqual(1, pf.CurrentLines);
        Assert.AreEqual(1, linesReported);
    }

    [UnityTest]
    public IEnumerator CurrentPieceType_IsValidAfterStart()
    {
        var pf = CreateController();
        yield return null;
        pf.StartGame();
        Assert.IsTrue(Enum.IsDefined(typeof(TetrominoType), pf.CurrentPieceType));
    }

    [UnityTest]
    public IEnumerator StartGame_PieceRotation_IsZeroInitially()
    {
        var pf = CreateController();
        yield return null;
        pf.StartGame();
        Assert.AreEqual(0, pf.CurrentPieceRotation);
    }

    [UnityTest]
    public IEnumerator GridHeight_Is20_GridWidth_Is10()
    {
        Assert.AreEqual(10, PlayfieldController.GridWidth);
        Assert.AreEqual(20, PlayfieldController.GridHeight);
        yield return null;
    }
}
