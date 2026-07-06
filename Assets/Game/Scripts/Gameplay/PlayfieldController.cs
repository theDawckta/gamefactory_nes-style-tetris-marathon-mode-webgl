using System;
using UnityEngine;

public class PlayfieldController : MonoBehaviour
{
    public const int GridWidth = 10;
    public const int GridHeight = 20;
    private const int SpawnX = 3;
    private const int SpawnY = 18;
    private const float LockDelaySeconds = 0.5f;
    private const int LockDelayMaxResets = 15;

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
    private TetrominoBag _bag;
    private TetrisInputHandler _input;
    private bool _isRunning;
    private bool _activePieceExists;
    private float _gravityAccumulator;
    private bool _inLockDelay;
    private float _lockDelayTimer;
    private int _lockDelayResets;
    private bool _lastActionWasRotation;

    private enum TSpinType { None, Mini, Full }

    private void Awake()
    {
        _input = GetComponent<TetrisInputHandler>();
    }

    public void StartGame()
    {
        _grid = new TetrominoType?[GridWidth, GridHeight];
        _bag = new TetrominoBag();
        CurrentScore = 0;
        CurrentLines = 0;
        CurrentLevel = 0;
        _gravityAccumulator = 0f;
        _inLockDelay = false;
        _lockDelayTimer = 0f;
        _lockDelayResets = 0;
        _lastActionWasRotation = false;
        _activePieceExists = false;
        _isRunning = true;

        NextPieceType = _bag.Next();
        SpawnPiece();
    }

    public void StopGame()
    {
        _isRunning = false;
        _activePieceExists = false;
    }

    public TetrominoType? GetCell(int x, int y)
    {
        if (x < 0 || x >= GridWidth || y < 0 || y >= GridHeight)
            return null;
        return _grid[x, y];
    }

    public Color GetCellColor(int x, int y)
    {
        var cell = GetCell(x, y);
        return cell.HasValue ? TetrominoData.GetColor(cell.Value) : Color.clear;
    }

    private void Update()
    {
        if (!_isRunning || !_activePieceExists)
            return;

        if (_inLockDelay)
            HandleLockDelayUpdate();
        else
            HandleGravityUpdate();
    }

    private void HandleGravityUpdate()
    {
        if (_input != null)
        {
            int dir = _input.MoveDirection;
            if (dir != 0)
                TryMove(dir, 0);

            if (_input.RotatePressedThisFrame)
                TryRotate();
        }

        bool softDropping = _input != null && _input.IsSoftDropping;
        float gravityInterval = NESGravityTable.GetSecondsPerRow(CurrentLevel);
        if (softDropping)
            gravityInterval = Mathf.Min(gravityInterval, 1f / 20f);

        _gravityAccumulator += Time.deltaTime;
        while (_gravityAccumulator >= gravityInterval)
        {
            _gravityAccumulator -= gravityInterval;
            bool moved = TryMoveDown();
            if (softDropping && moved)
                AddScore(1);
            if (!moved)
            {
                EnterLockDelay();
                break;
            }
            _lastActionWasRotation = false;
        }
    }

    private void HandleLockDelayUpdate()
    {
        if (_input != null)
        {
            int dir = _input.MoveDirection;
            if (dir != 0)
            {
                if (TryMove(dir, 0))
                {
                    _lockDelayTimer = 0f;
                    _lockDelayResets++;
                    if (_lockDelayResets >= LockDelayMaxResets)
                    {
                        LockPiece();
                        return;
                    }
                }
            }

            if (_input.RotatePressedThisFrame)
            {
                if (TryRotate())
                {
                    _lockDelayTimer = 0f;
                    _lockDelayResets++;
                    if (_lockDelayResets >= LockDelayMaxResets)
                    {
                        LockPiece();
                        return;
                    }
                }
            }
        }

        _lockDelayTimer += Time.deltaTime;
        if (_lockDelayTimer >= LockDelaySeconds)
            LockPiece();
    }

    private void EnterLockDelay()
    {
        _inLockDelay = true;
        _lockDelayTimer = 0f;
        _lockDelayResets = 0;
    }

    private void SpawnPiece()
    {
        CurrentPieceType = NextPieceType;
        NextPieceType = _bag.Next();
        CurrentPieceRotation = 0;
        CurrentPiecePosition = new Vector2Int(SpawnX, SpawnY);
        _gravityAccumulator = 0f;
        _inLockDelay = false;
        _lockDelayTimer = 0f;
        _lockDelayResets = 0;
        _lastActionWasRotation = false;

        if (HasCollision(CurrentPieceType, CurrentPieceRotation, CurrentPiecePosition))
        {
            _isRunning = false;
            _activePieceExists = false;
            OnGameOver?.Invoke();
            return;
        }

        _activePieceExists = true;
    }

    private void LockPiece()
    {
        TSpinType tspin = DetectTSpin();

        var cells = TetrominoData.GetCells(CurrentPieceType, CurrentPieceRotation);
        foreach (var cell in cells)
        {
            int gx = CurrentPiecePosition.x + cell.x;
            int gy = CurrentPiecePosition.y + cell.y;
            if (gx >= 0 && gx < GridWidth && gy >= 0 && gy < GridHeight)
                _grid[gx, gy] = CurrentPieceType;
        }

        _activePieceExists = false;
        _inLockDelay = false;
        OnPieceLocked?.Invoke();

        int linesCleared = ClearFullLines();
        int points = CalculateScore(linesCleared, tspin);
        AddScore(points);

        if (linesCleared > 0)
        {
            CurrentLines += linesCleared;
            OnLinesChanged?.Invoke(CurrentLines);

            int newLevel = CurrentLines / 10;
            if (newLevel != CurrentLevel)
            {
                CurrentLevel = newLevel;
                OnLevelChanged?.Invoke(CurrentLevel);
            }
        }

        SpawnPiece();
    }

    private int ClearFullLines()
    {
        int cleared = 0;
        for (int y = 0; y < GridHeight; y++)
        {
            bool full = true;
            for (int x = 0; x < GridWidth; x++)
            {
                if (!_grid[x, y].HasValue)
                {
                    full = false;
                    break;
                }
            }
            if (full)
            {
                cleared++;
                for (int row = y; row < GridHeight - 1; row++)
                    for (int col = 0; col < GridWidth; col++)
                        _grid[col, row] = _grid[col, row + 1];
                for (int col = 0; col < GridWidth; col++)
                    _grid[col, GridHeight - 1] = null;
                y--;
            }
        }
        return cleared;
    }

    private int CalculateScore(int lines, TSpinType tspin)
    {
        int m = CurrentLevel + 1;
        if (tspin == TSpinType.None)
        {
            switch (lines)
            {
                case 1: return 40 * m;
                case 2: return 100 * m;
                case 3: return 300 * m;
                case 4: return 1200 * m;
                default: return 0;
            }
        }
        if (tspin == TSpinType.Mini)
        {
            switch (lines)
            {
                case 0: return 100 * m;
                case 1: return 200 * m;
                case 2: return 1200 * m;
                case 3: return 1600 * m;
                default: return 0;
            }
        }
        // Full T-Spin
        switch (lines)
        {
            case 0: return 400 * m;
            case 1: return 800 * m;
            case 2: return 1200 * m;
            case 3: return 1600 * m;
            default: return 0;
        }
    }

    private bool TryMove(int dx, int dy)
    {
        var newPos = CurrentPiecePosition + new Vector2Int(dx, dy);
        if (HasCollision(CurrentPieceType, CurrentPieceRotation, newPos))
            return false;
        CurrentPiecePosition = newPos;
        if (dx != 0)
            _lastActionWasRotation = false;
        return true;
    }

    private bool TryMoveDown()
    {
        return TryMove(0, -1);
    }

    private bool TryRotate()
    {
        int newRot = (CurrentPieceRotation + 1) & 3;
        if (HasCollision(CurrentPieceType, newRot, CurrentPiecePosition))
            return false;
        CurrentPieceRotation = newRot;
        _lastActionWasRotation = true;
        return true;
    }

    private bool HasCollision(TetrominoType type, int rotation, Vector2Int pos)
    {
        var cells = TetrominoData.GetCells(type, rotation);
        foreach (var cell in cells)
        {
            int gx = pos.x + cell.x;
            int gy = pos.y + cell.y;
            if (gx < 0 || gx >= GridWidth || gy < 0 || gy >= GridHeight || _grid[gx, gy].HasValue)
                return true;
        }
        return false;
    }

    private bool IsCellOccupied(int x, int y)
    {
        if (x < 0 || x >= GridWidth || y < 0 || y >= GridHeight)
            return true;
        return _grid[x, y].HasValue;
    }

    // T-spin detection: only called at lock time when currentType == T.
    // Checks 4 diagonal corners around the T pivot for occupancy (out-of-bounds = occupied).
    // Front corners are on the side the stem points.
    // Full T-Spin: 3+ corners occupied AND both front occupied.
    // T-Spin Mini: 3+ corners occupied AND both back occupied (not both front).
    private TSpinType DetectTSpin()
    {
        if (CurrentPieceType != TetrominoType.T || !_lastActionWasRotation)
            return TSpinType.None;

        var p = CurrentPiecePosition;
        bool tl = IsCellOccupied(p.x - 1, p.y + 1);
        bool tr = IsCellOccupied(p.x + 1, p.y + 1);
        bool bl = IsCellOccupied(p.x - 1, p.y - 1);
        bool br = IsCellOccupied(p.x + 1, p.y - 1);

        int occupied = (tl ? 1 : 0) + (tr ? 1 : 0) + (bl ? 1 : 0) + (br ? 1 : 0);
        if (occupied < 3)
            return TSpinType.None;

        bool frontA, frontB;
        switch (CurrentPieceRotation & 3)
        {
            case 0: frontA = tl; frontB = tr; break; // stem up
            case 1: frontA = tr; frontB = br; break; // stem right
            case 2: frontA = bl; frontB = br; break; // stem down
            case 3: frontA = tl; frontB = bl; break; // stem left
            default: return TSpinType.None;
        }

        return (frontA && frontB) ? TSpinType.Full : TSpinType.Mini;
    }

    private void AddScore(int points)
    {
        if (points <= 0) return;
        CurrentScore += points;
        OnScoreChanged?.Invoke(CurrentScore);
    }
}
