using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

public class NextPieceWidget : MonoBehaviour
{
    [SerializeField] private PlayfieldController _playfieldController;
    [SerializeField] private FontAsset _font;

    // _cells[col, row]: col 0=left, row 0=top
    private VisualElement[,] _cells;
    private PlayfieldController _controller;
    private TetrominoType _lastType;

    private static readonly Color EmptyColor = new Color(0.05f, 0.05f, 0.05f);
    private static readonly Color EmptyBorder = new Color(0.15f, 0.15f, 0.15f);

    public void Initialize(VisualElement nextPieceRegion, PlayfieldController controller)
    {
        if (nextPieceRegion == null) return;

        _controller = controller != null ? controller : _playfieldController;

        var container = new VisualElement();
        container.style.flexDirection = FlexDirection.Column;
        container.style.paddingTop = 8f;
        container.style.paddingBottom = 8f;
        container.style.paddingLeft = 8f;
        container.style.paddingRight = 8f;

        var headerLabel = new Label("NEXT");
        headerLabel.style.color = new StyleColor(Color.white);
        headerLabel.style.unityTextAlign = TextAnchor.UpperCenter;
        if (_font != null)
            headerLabel.style.unityFontDefinition = new StyleFontDefinition(_font);

        var gridContainer = new VisualElement();
        gridContainer.style.position = Position.Relative;
        gridContainer.style.width = 80;
        gridContainer.style.height = 80;

        _cells = new VisualElement[4, 4];
        for (int row = 0; row < 4; row++)
        {
            for (int col = 0; col < 4; col++)
            {
                var cell = new VisualElement();
                cell.style.position = Position.Absolute;
                cell.style.left = col * 20;
                cell.style.top = row * 20;
                cell.style.width = 20;
                cell.style.height = 20;
                cell.style.backgroundColor = new StyleColor(EmptyColor);
                SetBorder(cell, EmptyBorder);
                gridContainer.Add(cell);
                _cells[col, row] = cell;
            }
        }

        container.Add(headerLabel);
        container.Add(gridContainer);
        nextPieceRegion.Add(container);

        if (_controller != null)
        {
            _controller.OnPieceLocked += HandlePieceLocked;
            _lastType = _controller.NextPieceType;
            Refresh(_lastType);
        }
    }

    private void Update()
    {
        if (_controller == null || _cells == null) return;
        var nextType = _controller.NextPieceType;
        if (nextType != _lastType)
        {
            _lastType = nextType;
            Refresh(nextType);
        }
    }

    private void HandlePieceLocked()
    {
        if (_controller == null || _cells == null) return;
        _lastType = _controller.NextPieceType;
        Refresh(_lastType);
    }

    private void Refresh(TetrominoType type)
    {
        if (_cells == null) return;

        for (int row = 0; row < 4; row++)
            for (int col = 0; col < 4; col++)
            {
                _cells[col, row].style.backgroundColor = new StyleColor(EmptyColor);
                SetBorder(_cells[col, row], EmptyBorder);
            }

        var offsets = TetrominoData.GetCells(type, 0);

        int minX = int.MaxValue, maxX = int.MinValue;
        int minY = int.MaxValue, maxY = int.MinValue;
        foreach (var c in offsets)
        {
            if (c.x < minX) minX = c.x;
            if (c.x > maxX) maxX = c.x;
            if (c.y < minY) minY = c.y;
            if (c.y > maxY) maxY = c.y;
        }

        int bboxW = maxX - minX + 1;
        int bboxH = maxY - minY + 1;
        int padX = (4 - bboxW) / 2;
        int padY = (4 - bboxH) / 2;

        Color pieceColor = TetrominoData.GetColor(type);

        foreach (var c in offsets)
        {
            int col = c.x - minX + padX;
            int row = (maxY - c.y) + padY;
            if (col >= 0 && col < 4 && row >= 0 && row < 4)
            {
                _cells[col, row].style.backgroundColor = new StyleColor(pieceColor);
                SetBorder(_cells[col, row], new Color(
                    Mathf.Max(0, pieceColor.r - 0.2f),
                    Mathf.Max(0, pieceColor.g - 0.2f),
                    Mathf.Max(0, pieceColor.b - 0.2f)));
            }
        }
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

    private void OnDestroy()
    {
        if (_controller != null)
            _controller.OnPieceLocked -= HandlePieceLocked;
    }
}
