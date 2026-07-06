using System;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

public class LinesWidget : MonoBehaviour
{
    [SerializeField] private PlayfieldController _playfieldController;
    [SerializeField] private FontAsset _font;

    private Label _valueLabel;
    private PlayfieldController _controller;

    public void Initialize(VisualElement linesRegion, PlayfieldController controller)
    {
        if (linesRegion == null) return;

        _controller = controller != null ? controller : _playfieldController;

        var container = new VisualElement();
        container.style.flexDirection = FlexDirection.Column;
        container.style.paddingTop = 8f;
        container.style.paddingBottom = 8f;
        container.style.paddingLeft = 8f;
        container.style.paddingRight = 8f;

        var headerLabel = new Label("LINES");
        headerLabel.style.color = new StyleColor(Color.white);
        headerLabel.style.unityTextAlign = TextAnchor.UpperCenter;
        if (_font != null)
            headerLabel.style.unityFontDefinition = new StyleFontDefinition(_font);

        _valueLabel = new Label("0");
        _valueLabel.style.color = new StyleColor(Color.white);
        _valueLabel.style.unityTextAlign = TextAnchor.UpperRight;
        if (_font != null)
            _valueLabel.style.unityFontDefinition = new StyleFontDefinition(_font);

        container.Add(headerLabel);
        container.Add(_valueLabel);
        linesRegion.Add(container);

        if (_controller != null)
            _controller.OnLinesChanged += HandleLinesChanged;
    }

    private void HandleLinesChanged(int lines)
    {
        if (_valueLabel != null)
            _valueLabel.text = lines.ToString();
    }

    private void OnDestroy()
    {
        if (_controller != null)
            _controller.OnLinesChanged -= HandleLinesChanged;
    }
}
