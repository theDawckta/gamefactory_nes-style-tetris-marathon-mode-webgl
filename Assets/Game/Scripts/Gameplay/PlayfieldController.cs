using System;
using UnityEngine;

public class PlayfieldController : MonoBehaviour
{
    public const int GridWidth = 10;
    public const int GridHeight = 20;

    public event Action<int> OnScoreChanged;
    public event Action<int> OnLinesChanged;
    public event Action<int> OnLevelChanged;
    public event Action OnGameOver;
    public event Action OnPieceLocked;

    public int CurrentScore { get; private set; }
    public int CurrentLines { get; private set; }
    public int CurrentLevel { get; private set; }
    public TetrominoType NextPieceType { get; private set; }
    public TetrominoType CurrentPieceType { get; private set; }
    public int CurrentPieceRotation { get; private set; }
    public Vector2Int CurrentPiecePosition { get; private set; }

    private TetrominoType?[,] _grid;
    private bool _isRunning;
    private bool _isLockDelay;
    private float _gravityAccum;
    private bool _wasSoftDropping;
    private float _lockDelayTimer;
    private int _lockResetCount;
    private bool _lastActionWasRotation;
    private TetrominoBag _bag;
    private TetrisInputHandler _input;

    private const float LockDelayDuration = 0.5f;
    private const int LockResetMax = 15;
    private const float SoftDropInterval = 0.05f; // 1/20 s

    private void Awake()
    {
        _grid = new TetrominoType?[GridWidth, GridHeight];
        _input = GetComponent<TetrisInputHandler>();
    }

    public void StartGame()
    {
        for (int x = 0; x < GridWidth; x++)
            for (int y = 0; y < GridHeight; y++)
                _grid[x, y] = null;

        CurrentScore = 0;
        CurrentLines = 0;
        CurrentLevel = 0;
        _gravityAccum = 0f;
        _wasSoftDropping = false;
        _isLockDelay = false;
        _lockDelayTimer = 0f;
        _lockResetCount = 0;
        _lastActionWasRotation = false;
        _bag = new TetrominoBag();
        NextPieceType = _bag.Next();
        _isRunning = true;
        SpawnNextPiece();
    }

    public void StopGame()
    {
        _isRunning = false;
    }

    public bool IsRunning => _isRunning;

    public void Pause() => _isRunning = false;

    public void Resume() => _isRunning = true;

    private void Update()
    {
        if (!_isRunning) return;
        HandleInput();
        HandleGravity();
    }

    private void HandleInput()
    {
        if (_input == null) return;

        if (_input.MoveDirection != 0)
        {
            var newPos = CurrentPiecePosition + new Vector2Int(_input.MoveDirection, 0);
            if (!Collides(CurrentPieceType, CurrentPieceRotation, newPos))
            {
                CurrentPiecePosition = newPos;
                _lastActionWasRotation = false;
                if (_isLockDelay) ResetLockDelay();
            }
        }

        if (_input.RotatePressedThisFrame)
        {
            int newRot = (CurrentPieceRotation + 1) & 3;
            if (!Collides(CurrentPieceType, newRot, CurrentPiecePosition))
            {
                CurrentPieceRotation = newRot;
                _lastActionWasRotation = true;
                if (_isLockDelay) ResetLockDelay();
            }
        }
    }

    private void ResetLockDelay()
    {
        _lockResetCount++;
        if (_lockResetCount >= LockResetMax)
            LockPiece();
        else
            _lockDelayTimer = 0f;
    }

    private void HandleGravity()
    {
        if (_isLockDelay)
        {
            _lockDelayTimer += Time.deltaTime;
            if (_lockDelayTimer >= LockDelayDuration)
                LockPiece();
            return;
        }

        bool softDrop = _input != null && _input.IsSoftDropping;

        // Reset the accumulator whenever soft-drop toggles. Without this, slow gravity that has
        // been building up (e.g. ~0.7s at level 0) would all convert into fast 0.05s steps the
        // instant Down is pressed, slamming the piece down many rows at once (the "instant drop"
        // bug). Starting fresh on the transition keeps the drop speed consistent.
        if (softDrop != _wasSoftDropping)
        {
            _gravityAccum = 0f;
            _wasSoftDropping = softDrop;
        }

        float interval = softDrop
            ? Mathf.Min(NESGravityTable.GetSecondsPerRow(CurrentLevel), SoftDropInterval)
            : NESGravityTable.GetSecondsPerRow(CurrentLevel);

        _gravityAccum += Time.deltaTime;
        int steps = 0;
        while (_gravityAccum >= interval && !_isLockDelay && steps < 20)
        {
            _gravityAccum -= interval;
            steps++;
            TryMoveDown(softDrop);
        }
    }

    private void TryMoveDown(bool awardSoftDropPoint)
    {
        var newPos = CurrentPiecePosition + new Vector2Int(0, -1);
        if (!Collides(CurrentPieceType, CurrentPieceRotation, newPos))
        {
            CurrentPiecePosition = newPos;
            _lastActionWasRotation = false;
            if (awardSoftDropPoint)
            {
                CurrentScore++;
                OnScoreChanged?.Invoke(CurrentScore);
            }
        }
        else
        {
            _isLockDelay = true;
            _lockDelayTimer = 0f;
            _lockResetCount = 0;
        }
    }

    private void LockPiece()
    {
        _isLockDelay = false;

        var cells = TetrominoData.GetCells(CurrentPieceType, CurrentPieceRotation);
        foreach (var cell in cells)
        {
            int x = CurrentPiecePosition.x + cell.x;
            int y = CurrentPiecePosition.y + cell.y;
            if (x >= 0 && x < GridWidth && y >= 0 && y < GridHeight)
                _grid[x, y] = CurrentPieceType;
        }

        OnPieceLocked?.Invoke();

        var tSpin = DetectTSpin();
        int linesCleared = ClearLines();

        int score = CalculateScore(linesCleared, tSpin);
        if (score > 0)
        {
            CurrentScore += score;
            OnScoreChanged?.Invoke(CurrentScore);
        }

        int newLevel = CurrentLines / 10;
        if (newLevel != CurrentLevel)
        {
            CurrentLevel = newLevel;
            OnLevelChanged?.Invoke(CurrentLevel);
        }

        SpawnNextPiece();
    }

    private int ClearLines()
    {
        int cleared = 0;
        for (int y = 0; y < GridHeight; y++)
        {
            if (IsLineFull(y))
            {
                RemoveLine(y);
                cleared++;
                y--;
            }
        }
        if (cleared > 0)
        {
            CurrentLines += cleared;
            OnLinesChanged?.Invoke(CurrentLines);
        }
        return cleared;
    }

    private bool IsLineFull(int y)
    {
        for (int x = 0; x < GridWidth; x++)
            if (_grid[x, y] == null) return false;
        return true;
    }

    private void RemoveLine(int y)
    {
        for (int row = y; row < GridHeight - 1; row++)
            for (int x = 0; x < GridWidth; x++)
                _grid[x, row] = _grid[x, row + 1];
        for (int x = 0; x < GridWidth; x++)
            _grid[x, GridHeight - 1] = null;
    }

    private enum TSpinType { None, Mini, Full }

    private TSpinType DetectTSpin()
    {
        if (CurrentPieceType != TetrominoType.T || !_lastActionWasRotation)
            return TSpinType.None;

        int px = CurrentPiecePosition.x;
        int py = CurrentPiecePosition.y;

        bool tl = IsOccupiedOrOOB(px - 1, py + 1);
        bool tr = IsOccupiedOrOOB(px + 1, py + 1);
        bool bl = IsOccupiedOrOOB(px - 1, py - 1);
        bool br = IsOccupiedOrOOB(px + 1, py - 1);

        int count = (tl ? 1 : 0) + (tr ? 1 : 0) + (bl ? 1 : 0) + (br ? 1 : 0);
        if (count < 3) return TSpinType.None;

        bool f0, f1;
        switch (CurrentPieceRotation)
        {
            case 0: f0 = tl; f1 = tr; break; // stem up:    front = top corners
            case 1: f0 = tr; f1 = br; break; // stem right: front = right corners
            case 2: f0 = bl; f1 = br; break; // stem down:  front = bottom corners
            case 3: f0 = tl; f1 = bl; break; // stem left:  front = left corners
            default: return TSpinType.None;
        }

        return (f0 && f1) ? TSpinType.Full : TSpinType.Mini;
    }

    private bool IsOccupiedOrOOB(int x, int y)
    {
        if (x < 0 || x >= GridWidth || y < 0 || y >= GridHeight) return true;
        return _grid[x, y] != null;
    }

    private int CalculateScore(int lines, TSpinType tSpin)
    {
        int mult = CurrentLevel + 1;

        if (tSpin == TSpinType.Full)
        {
            int[] table = { 400, 800, 1200, 1600 };
            return table[Mathf.Min(lines, table.Length - 1)] * mult;
        }

        if (tSpin == TSpinType.Mini)
        {
            switch (lines)
            {
                case 0: return 100 * mult;
                case 1: return 200 * mult;
                case 2: return 1200 * mult; // score as T-spin Double
                case 3: return 1600 * mult; // score as T-spin Triple
                default: return 0;
            }
        }

        int[] lineScores = { 0, 40, 100, 300, 1200 };
        return lineScores[Mathf.Min(lines, lineScores.Length - 1)] * mult;
    }

    private void SpawnNextPiece()
    {
        CurrentPieceType = NextPieceType;
        CurrentPieceRotation = 0;
        CurrentPiecePosition = new Vector2Int(3, 18);
        _lastActionWasRotation = false;
        _gravityAccum = 0f;
        _isLockDelay = false;
        _lockDelayTimer = 0f;
        _lockResetCount = 0;
        NextPieceType = _bag.Next();

        if (Collides(CurrentPieceType, CurrentPieceRotation, CurrentPiecePosition))
        {
            _isRunning = false;
            OnGameOver?.Invoke();
        }
    }

    private bool Collides(TetrominoType type, int rotation, Vector2Int pos)
    {
        var cells = TetrominoData.GetCells(type, rotation);
        foreach (var cell in cells)
        {
            int x = pos.x + cell.x;
            int y = pos.y + cell.y;
            if (x < 0 || x >= GridWidth || y < 0) return true;
            if (y < GridHeight && _grid[x, y] != null) return true;
        }
        return false;
    }

    public void MoveLeft()
    {
        if (!_isRunning) return;
        var newPos = CurrentPiecePosition + new Vector2Int(-1, 0);
        if (!Collides(CurrentPieceType, CurrentPieceRotation, newPos))
        {
            CurrentPiecePosition = newPos;
            _lastActionWasRotation = false;
            if (_isLockDelay) ResetLockDelay();
        }
    }

    public void MoveRight()
    {
        if (!_isRunning) return;
        var newPos = CurrentPiecePosition + new Vector2Int(1, 0);
        if (!Collides(CurrentPieceType, CurrentPieceRotation, newPos))
        {
            CurrentPiecePosition = newPos;
            _lastActionWasRotation = false;
            if (_isLockDelay) ResetLockDelay();
        }
    }

    // Moves the piece down one step. Resets _gravityAccum so gravity does not also fire
    // immediately after, which would double-drop the piece on the same frame.
    public void SoftDrop()
    {
        if (!_isRunning || _isLockDelay) return;
        TryMoveDown(true);
        _gravityAccum = 0f;
    }

    public void Rotate()
    {
        if (!_isRunning) return;
        int newRot = (CurrentPieceRotation + 1) & 3;
        if (!Collides(CurrentPieceType, newRot, CurrentPiecePosition))
        {
            CurrentPieceRotation = newRot;
            _lastActionWasRotation = true;
            if (_isLockDelay) ResetLockDelay();
        }
    }

    public TetrominoType? GetCell(int x, int y)
    {
        if (x < 0 || x >= GridWidth || y < 0 || y >= GridHeight) return null;
        return _grid[x, y];
    }

    public Color GetCellColor(int x, int y)
    {
        var cell = GetCell(x, y);
        if (cell == null) return Color.clear;
        return TetrominoData.GetColor(cell.Value);
    }

}
