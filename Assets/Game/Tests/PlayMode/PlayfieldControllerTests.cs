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

    private void SetPieceType(TetrominoType type)
    {
        var field = typeof(PlayfieldController).GetField("<CurrentPieceType>k__BackingField",
            BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(_controller, type);
    }

    private void SetPieceRotation(int rotation)
    {
        var field = typeof(PlayfieldController).GetField("<CurrentPieceRotation>k__BackingField",
            BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(_controller, rotation);
    }

    private void SetGridCell(int x, int y, TetrominoType type)
    {
        var gridField = typeof(PlayfieldController).GetField("_grid", BindingFlags.NonPublic | BindingFlags.Instance);
        var grid = (TetrominoType?[,])gridField.GetValue(_controller);
        grid[x, y] = type;
    }

    // Places the active piece deterministically (type/rotation/position) for a kick test.
    private void PlacePiece(TetrominoType type, int rotation, Vector2Int pos)
    {
        SetPieceType(type);
        SetPieceRotation(rotation);
        SetPiecePosition(pos);
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
    public IEnumerator StartGame_SpawnsPieceAtColumn4Row18()
    {
        _controller.StartGame();
        yield return null;
        Assert.AreEqual(4, _controller.CurrentPiecePosition.x);
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
        // Locking the first piece at its default spawn position (4,18) fills
        // grid cells there. The subsequent spawn at (4,18) collides, triggering OnGameOver.
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

    // ── SRS wall kicks ─────────────────────────────────────────────────────────

    [UnityTest]
    public IEnumerator Rotate_InOpenSpace_UsesNoKick_PositionUnchanged()
    {
        _controller.StartGame();
        yield return null;
        PlacePiece(TetrominoType.T, 0, new Vector2Int(5, 10));
        _controller.Rotate();
        Assert.AreEqual(1, _controller.CurrentPieceRotation);
        Assert.AreEqual(new Vector2Int(5, 10), _controller.CurrentPiecePosition);
    }

    [UnityTest]
    public IEnumerator Rotate_Blocked_KicksLeftAndUp()
    {
        // T in spawn state at (5,10). Blockers below-center (5,9) and (4,9) make the
        // in-place rotation (test 1) and the left kick (test 2) both collide, so the
        // JLSTZ 0->1 test 3 offset (-1,+1) resolves it: left one, UP one. This pins
        // BOTH the x and y signs of the kick table.
        _controller.StartGame();
        yield return null;
        SetGridCell(5, 9, TetrominoType.I);
        SetGridCell(4, 9, TetrominoType.I);
        PlacePiece(TetrominoType.T, 0, new Vector2Int(5, 10));
        _controller.Rotate();
        Assert.AreEqual(1, _controller.CurrentPieceRotation);
        Assert.AreEqual(new Vector2Int(4, 11), _controller.CurrentPiecePosition);
    }

    [UnityTest]
    public IEnumerator Rotate_IPiece_Blocked_UsesIKickTable()
    {
        // I horizontal at (5,10); blocker at (6,9) forces the vertical rotation off its
        // in-place column via the I-table 0->1 test 2 offset (-2,0): kick left by two.
        _controller.StartGame();
        yield return null;
        SetGridCell(6, 9, TetrominoType.I);
        PlacePiece(TetrominoType.I, 0, new Vector2Int(5, 10));
        _controller.Rotate();
        Assert.AreEqual(1, _controller.CurrentPieceRotation);
        Assert.AreEqual(new Vector2Int(3, 10), _controller.CurrentPiecePosition);
    }

    [UnityTest]
    public IEnumerator RotateCCW_InOpenSpace_DecrementsRotation()
    {
        _controller.StartGame();
        yield return null;
        PlacePiece(TetrominoType.T, 0, new Vector2Int(5, 10));
        _controller.RotateCCW();
        Assert.AreEqual(3, _controller.CurrentPieceRotation);
        Assert.AreEqual(new Vector2Int(5, 10), _controller.CurrentPiecePosition);
    }

    [UnityTest]
    public IEnumerator Rotate_OPiece_NeverChangesPosition()
    {
        _controller.StartGame();
        yield return null;
        PlacePiece(TetrominoType.O, 0, new Vector2Int(5, 10));
        _controller.Rotate();
        Assert.AreEqual(new Vector2Int(5, 10), _controller.CurrentPiecePosition);
    }

    // ── T-spin ─────────────────────────────────────────────────────────────────

    // Fills three of the four diagonal corners around center (5,5) and places a T in
    // its R state there, ready to rotate CW (in place) into the stem-down T-spin slot.
    private void SetUpTSpinSlot()
    {
        SetGridCell(4, 6, TetrominoType.I); // top-left corner
        SetGridCell(4, 4, TetrominoType.I); // bottom-left corner
        SetGridCell(6, 4, TetrominoType.I); // bottom-right corner
        PlacePiece(TetrominoType.T, 1, new Vector2Int(5, 5));
    }

    [UnityTest]
    public IEnumerator Rotate_IntoThreeCornerSlot_ScoresTSpin()
    {
        _controller.StartGame();
        yield return null;
        SetUpTSpinSlot();
        _controller.Rotate();                 // R -> down (stem into the notch)
        InvokeLockPiece();
        // T-spin (Full) with 0 lines at level 0 = 400.
        Assert.AreEqual(400, _controller.CurrentScore);
    }

    [UnityTest]
    public IEnumerator HardDrop_AfterTSpinRotation_WhenSeated_KeepsTSpinScore()
    {
        // Rotating into the snug slot leaves the piece resting (drop distance 0), so the
        // hard drop must preserve the rotation flag and still credit the T-spin.
        _controller.StartGame();
        yield return null;
        SetUpTSpinSlot();
        _controller.Rotate();
        _controller.HardDrop();
        Assert.AreEqual(400, _controller.CurrentScore);
    }

    [UnityTest]
    public IEnumerator HardDrop_AfterRotation_WithSpaceBelow_DoesNotScoreTSpin()
    {
        // T rotated in open space then hard-dropped: it actually falls, so the rotation
        // flag is cleared and only the hard-drop bonus (+2/cell) is scored, not a T-spin.
        _controller.StartGame();
        yield return null;
        PlacePiece(TetrominoType.T, 1, new Vector2Int(5, 10));
        _controller.Rotate();  // stays at (5,10), flag set
        _controller.HardDrop(); // falls 9 rows to (5,1) -> flag cleared
        Assert.AreEqual(18, _controller.CurrentScore); // 9 * 2, no T-spin bonus
    }

    // ── Hard drop ──────────────────────────────────────────────────────────────

    [UnityTest]
    public IEnumerator HardDrop_LocksImmediately_AndSpawnsNext()
    {
        bool locked = false;
        _controller.OnPieceLocked += () => locked = true;
        _controller.StartGame();
        yield return null;
        _controller.HardDrop();
        Assert.IsTrue(locked, "HardDrop must lock synchronously, bypassing lock delay");
        Assert.AreEqual(new Vector2Int(4, 18), _controller.CurrentPiecePosition);
    }

    [UnityTest]
    public IEnumerator HardDrop_WhenNotRunning_DoesNothing()
    {
        _controller.StartGame();
        yield return null;
        _controller.StopGame();
        var startPos = _controller.CurrentPiecePosition;
        _controller.HardDrop();
        Assert.AreEqual(startPos, _controller.CurrentPiecePosition);
    }

    [UnityTest]
    public IEnumerator GetGhostPosition_OverEmptyColumn_LandsOnFloor()
    {
        _controller.StartGame();
        yield return null;
        PlacePiece(TetrominoType.I, 0, new Vector2Int(4, 10));
        Assert.AreEqual(new Vector2Int(4, 0), _controller.GetGhostPosition());
    }

    [UnityTest]
    public IEnumerator GetGhostPosition_OverStack_LandsOnTopOfStack()
    {
        _controller.StartGame();
        yield return null;
        FillRow(0);
        PlacePiece(TetrominoType.I, 0, new Vector2Int(4, 10));
        Assert.AreEqual(new Vector2Int(4, 1), _controller.GetGhostPosition());
    }
}
