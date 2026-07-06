using UnityEngine;
using UnityEngine.UIElements;

public class PlayfieldRenderer : MonoBehaviour
{
    [SerializeField] private PlayfieldController _playfieldController;

    private VisualElement _region;
    private VisualElement[,] _cellElements;
    private bool _active;

    private static readonly Color EmptyFill = new Color(0.05f, 0.05f, 0.05f);
    private static readonly Color EmptyBorder = new Color(0.15f, 0.15f, 0.15f);

    public void Initialize(VisualElement playfieldRegion, PlayfieldController controller)
    {
        _region = playfieldRegion;
        _playfieldController = controller;

        _region.Clear();
        _cellElements = new VisualElement[PlayfieldController.GridWidth, PlayfieldController.GridHeight];

        for (int y = 0; y < PlayfieldController.GridHeight; y++)
        {
            for (int x = 0; x < PlayfieldController.GridWidth; x++)
            {
                var cell = new VisualElement();
                cell.style.position = Position.Absolute;
                cell.style.left = x * 20;
                cell.style.top = (19 - y) * 20;
                cell.style.width = 20;
                cell.style.height = 20;
                ApplyEmptyStyle(cell);
                _region.Add(cell);
                _cellElements[x, y] = cell;
            }
        }
    }

    public void SetActive(bool active)
    {
        _active = active;
    }

    private void Update()
    {
        if (!_active || _playfieldController == null || _cellElements == null) return;
        RenderFrame();
    }

    private void RenderFrame()
    {
        var activeCells = new bool[PlayfieldController.GridWidth, PlayfieldController.GridHeight];
        var pieces = TetrominoData.GetCells(_playfieldController.CurrentPieceType, _playfieldController.CurrentPieceRotation);
        var piecePos = _playfieldController.CurrentPiecePosition;
        Color rawPieceColor = TetrominoData.GetColor(_playfieldController.CurrentPieceType);
        // Slightly brighter tint to distinguish active piece from locked cells
        Color activePieceColor = Color.Lerp(rawPieceColor, Color.white, 0.25f);

        foreach (var c in pieces)
        {
            int cx = piecePos.x + c.x;
            int cy = piecePos.y + c.y;
            if (cx >= 0 && cx < PlayfieldController.GridWidth && cy >= 0 && cy < PlayfieldController.GridHeight)
                activeCells[cx, cy] = true;
        }

        for (int y = 0; y < PlayfieldController.GridHeight; y++)
        {
            for (int x = 0; x < PlayfieldController.GridWidth; x++)
            {
                var cellEl = _cellElements[x, y];

                if (activeCells[x, y])
                {
                    cellEl.style.backgroundColor = new StyleColor(activePieceColor);
                    SetBorder(cellEl, DarkenBorder(activePieceColor));
                }
                else
                {
                    Color locked = _playfieldController.GetCellColor(x, y);
                    if (locked == Color.clear)
                    {
                        ApplyEmptyStyle(cellEl);
                    }
                    else
                    {
                        cellEl.style.backgroundColor = new StyleColor(locked);
                        SetBorder(cellEl, DarkenBorder(locked));
                    }
                }
            }
        }
    }

    private static Color DarkenBorder(Color fill)
    {
        return new Color(
            Mathf.Max(0, fill.r - 0.2f),
            Mathf.Max(0, fill.g - 0.2f),
            Mathf.Max(0, fill.b - 0.2f));
    }

    private static void ApplyEmptyStyle(VisualElement cell)
    {
        cell.style.backgroundColor = new StyleColor(EmptyFill);
        SetBorder(cell, EmptyBorder);
    }

    private static void SetBorder(VisualElement cell, Color color)
    {
        cell.style.borderTopWidth = 1;
        cell.style.borderBottomWidth = 1;
        cell.style.borderLeftWidth = 1;
        cell.style.borderRightWidth = 1;
        cell.style.borderTopColor = new StyleColor(color);
        cell.style.borderBottomColor = new StyleColor(color);
        cell.style.borderLeftColor = new StyleColor(color);
        cell.style.borderRightColor = new StyleColor(color);
    }
}
