using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class PlayfieldControllerTests
{
    private GameObject _go;
    private PlayfieldController _controller;

    [SetUp]
    public void SetUp()
    {
        _go = new GameObject();
        _controller = _go.AddComponent<PlayfieldController>();
    }

    [TearDown]
    public void TearDown()
    {
        UnityEngine.Object.Destroy(_go);
    }

    private void FillRow(int y)
    {
        var gridField = typeof(PlayfieldController).GetField("_grid", BindingFlags.NonPublic | BindingFlags.Instance);
        var grid = (TetrominoType?[,])gridField.GetValue(_controller);
        for (int x = 0; x < PlayfieldController.GridWidth; x++)
            grid[x, y] = TetrominoType.I;
    }

    private void InvokeLockPiece()
    {
        var method = typeof(PlayfieldController).GetMethod("LockPiece", BindingFlags.NonPublic | BindingFlags.Instance);
        method.Invoke(_controller, null);
    }

    private void SetPiecePosition(Vector2Int pos)
    {
        var field = typeof(PlayfieldController).GetField("<CurrentPiecePosition>k__BackingField",
            BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(_controller, pos);
    }

    // ── Constants ─────────────────────────────────────────────────────────────

    [UnityTest]
    public IEnumerator GridWidth_IsTen()
    {
        yield return null;
        Assert.AreEqual(10, PlayfieldController.GridWidth);
    }

    [UnityTest]
    public IEnumerator GridHeight_IsTwenty()
    {
        yield return null;
        Assert.AreEqual(20, PlayfieldController.GridHeight);
    }

    // ── StartGame ─────────────────────────────────────────────────────────────

    [UnityTest]
    public IEnumerator StartGame_SpawnsPieceAtColumn3Row18()
    {
        _controller.StartGame();
        yield return null;
        Assert.AreEqual(3, _controller.CurrentPiecePosition.x);
        Assert.AreEqual(18, _controller.CurrentPiecePosition.y);
    }

    [UnityTest]
    public IEnumerator StartGame_InitialScoreIsZero()
    {
        _controller.StartGame();
        yield return null;
        Assert.AreEqual(0, _controller.CurrentScore);
    }

    [UnityTest]
    public IEnumerator StartGame_InitialLinesIsZero()
    {
        _controller.StartGame();
        yield return null;
        Assert.AreEqual(0, _controller.CurrentLines);
    }

    [UnityTest]
    public IEnumerator StartGame_InitialLevelIsZero()
    {
        _controller.StartGame();
        yield return null;
        Assert.AreEqual(0, _controller.CurrentLevel);
    }

    [UnityTest]
    public IEnumerator StartGame_CurrentPieceTypeIsValidEnum()
    {
        _controller.StartGame();
        yield return null;
        Assert.IsTrue(Enum.IsDefined(typeof(TetrominoType), _controller.CurrentPieceType));
    }

    [UnityTest]
    public IEnumerator StartGame_InitialRotationIsZero()
    {
        _controller.StartGame();
        yield return null;
        Assert.AreEqual(0, _controller.CurrentPieceRotation);
    }

    [UnityTest]
    public IEnumerator StartGame_NextPieceTypeIsValidEnum()
    {
        _controller.StartGame();
        yield return null;
        Assert.IsTrue(Enum.IsDefined(typeof(TetrominoType), _controller.NextPieceType));
    }

    // ── GetCell / GetCellColor ─────────────────────────────────────────────────

    [UnityTest]
    public IEnumerator GetCell_OutOfBounds_ReturnsNull()
    {
        _controller.StartGame();
        yield return null;
        Assert.IsNull(_controller.GetCell(-1, 0));
        Assert.IsNull(_controller.GetCell(0, -1));
        Assert.IsNull(_controller.GetCell(PlayfieldController.GridWidth, 0));
        Assert.IsNull(_controller.GetCell(0, PlayfieldController.GridHeight));
    }

    [UnityTest]
    public IEnumerator GetCell_EmptyCell_ReturnsNull()
    {
        _controller.StartGame();
        yield return null;
        Assert.IsNull(_controller.GetCell(0, 0));
    }

    [UnityTest]
    public IEnumerator GetCellColor_EmptyCell_ReturnsClear()
    {
        _controller.StartGame();
        yield return null;
        Assert.AreEqual(Color.clear, _controller.GetCellColor(0, 0));
    }

    [UnityTest]
    public IEnumerator GetCellColor_LockedCell_ReturnsCorrectColor()
    {
        _controller.StartGame();
        yield return null;
        FillRow(0); // fills with TetrominoType.I (cyan)
        Assert.AreEqual(TetrominoData.GetColor(TetrominoType.I), _controller.GetCellColor(0, 0));
    }

    // ── StopGame ──────────────────────────────────────────────────────────────

    [UnityTest]
    public IEnumerator StopGame_HaltsGameplay_NoGameOverFires()
    {
        bool gameOverFired = false;
        _controller.OnGameOver += () => gameOverFired = true;
        _controller.StartGame();
        _controller.StopGame();
        for (int i = 0; i < 5; i++) yield return null;
        Assert.IsFalse(gameOverFired, "OnGameOver must not fire after StopGame");
    }

    // ── Game Over ─────────────────────────────────────────────────────────────

    [UnityTest]
    public IEnumerator OnGameOver_FiresWhenSpawnCollides()
    {
        // Locking the first piece at its default spawn position (3,18) fills
        // grid cells there. The subsequent spawn at (3,18) collides, triggering OnGameOver.
        bool fired = false;
        _controller.OnGameOver += () => fired = true;
        _controller.StartGame();
        yield return null;
        InvokeLockPiece();
        Assert.IsTrue(fired);
    }

    // ── OnPieceLocked ─────────────────────────────────────────────────────────

    [UnityTest]
    public IEnumerator OnPieceLocked_FiresOnLock()
    {
        bool fired = false;
        _controller.OnPieceLocked += () => fired = true;
        _controller.StartGame();
        yield return null;
        InvokeLockPiece();
        Assert.IsTrue(fired);
    }

    // ── Scoring ───────────────────────────────────────────────────────────────

    [UnityTest]
    public IEnumerator OnScoreChanged_FiresOnLineClear()
    {
        bool fired = false;
        _controller.OnScoreChanged += _ => fired = true;
        _controller.StartGame();
        yield return null;
        FillRow(0);
        SetPiecePosition(new Vector2Int(3, 5));
        InvokeLockPiece();
        Assert.IsTrue(fired);
    }

    [UnityTest]
    public IEnumerator SingleLineClear_Awards40xLevelMultiplier()
    {
        _controller.StartGame();
        yield return null;
        FillRow(0);
        SetPiecePosition(new Vector2Int(3, 5));
        InvokeLockPiece();
        // Level 0: 40 * (0+1) = 40
        Assert.AreEqual(40, _controller.CurrentScore);
    }

    [UnityTest]
    public IEnumerator DoubleLineClear_Awards100xLevelMultiplier()
    {
        _controller.StartGame();
        yield return null;
        FillRow(0);
        FillRow(1);
        SetPiecePosition(new Vector2Int(3, 5));
        InvokeLockPiece();
        // Level 0: 100 * (0+1) = 100
        Assert.AreEqual(100, _controller.CurrentScore);
    }

    [UnityTest]
    public IEnumerator TetrisLineClear_Awards1200xLevelMultiplier()
    {
        _controller.StartGame();
        yield return null;
        FillRow(0); FillRow(1); FillRow(2); FillRow(3);
        SetPiecePosition(new Vector2Int(3, 6));
        InvokeLockPiece();
        // Level 0: 1200 * (0+1) = 1200
        Assert.AreEqual(1200, _controller.CurrentScore);
    }

    // ── Lines / Level ─────────────────────────────────────────────────────────

    [UnityTest]
    public IEnumerator OnLinesChanged_FiresWithCorrectCount()
    {
        int reportedLines = -1;
        _controller.OnLinesChanged += count => reportedLines = count;
        _controller.StartGame();
        yield return null;
        FillRow(0);
        SetPiecePosition(new Vector2Int(3, 5));
        InvokeLockPiece();
        Assert.AreEqual(1, reportedLines);
    }

    [UnityTest]
    public IEnumerator LevelIncrement_EveryTenLines()
    {
        _controller.StartGame();
        yield return null;
        // Fill rows 0-9 (10 complete rows); piece placed at row 12 avoids spawn area
        for (int r = 0; r < 10; r++) FillRow(r);
        SetPiecePosition(new Vector2Int(3, 12));
        InvokeLockPiece();
        Assert.AreEqual(1, _controller.CurrentLevel);
    }

    [UnityTest]
    public IEnumerator OnLevelChanged_FiresWhenLevelIncrements()
    {
        int reportedLevel = -1;
        _controller.OnLevelChanged += lvl => reportedLevel = lvl;
        _controller.StartGame();
        yield return null;
        for (int r = 0; r < 10; r++) FillRow(r);
        SetPiecePosition(new Vector2Int(3, 12));
        InvokeLockPiece();
        Assert.AreEqual(1, reportedLevel);
    }
}
