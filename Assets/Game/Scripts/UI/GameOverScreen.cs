using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class GameOverScreen : BaseScreen
{
    [SerializeField] private FontAsset _font;

    public event Action OnReturnPressed;

    private Label _scoreValueLabel;
    private Label _promptLabel;
    private Coroutine _blinkCoroutine;
    private int _shownFrame = -1;

    private void Start()
    {
        BuildUI();
    }

    private void BuildUI()
    {
        var root = Root;
        if (root == null) return;

        root.Clear();
        root.style.flexDirection = FlexDirection.Column;
        root.style.alignItems = Align.Center;
        root.style.justifyContent = Justify.Center;
        root.style.backgroundColor = new StyleColor(new Color(0f, 0f, 0f, 0.85f));
        root.style.width = new StyleLength(new Length(100f, LengthUnit.Percent));
        root.style.height = new StyleLength(new Length(100f, LengthUnit.Percent));

        var gameOverLabel = new Label("GAME OVER");
        gameOverLabel.style.fontSize = 72;
        gameOverLabel.style.color = new StyleColor(Color.white);
        gameOverLabel.style.marginBottom = 32;
        if (_font != null) gameOverLabel.style.unityFontDefinition = new StyleFontDefinition(_font);
        root.Add(gameOverLabel);

        var finalScoreRegion = new VisualElement();
        finalScoreRegion.name = "finalScoreRegion";
        finalScoreRegion.style.flexDirection = FlexDirection.Column;
        finalScoreRegion.style.alignItems = Align.Center;
        finalScoreRegion.style.marginBottom = 16;

        var scoreHeaderLabel = new Label("SCORE");
        scoreHeaderLabel.style.fontSize = 28;
        scoreHeaderLabel.style.color = new StyleColor(Color.white);
        if (_font != null) scoreHeaderLabel.style.unityFontDefinition = new StyleFontDefinition(_font);
        finalScoreRegion.Add(scoreHeaderLabel);

        _scoreValueLabel = new Label("0");
        _scoreValueLabel.name = "scoreValue";
        _scoreValueLabel.style.fontSize = 36;
        _scoreValueLabel.style.color = new StyleColor(Color.white);
        if (_font != null) _scoreValueLabel.style.unityFontDefinition = new StyleFontDefinition(_font);
        finalScoreRegion.Add(_scoreValueLabel);

        root.Add(finalScoreRegion);

        var highScoreBannerRegion = new VisualElement();
        highScoreBannerRegion.name = "highScoreBannerRegion";
        highScoreBannerRegion.style.flexDirection = FlexDirection.Column;
        highScoreBannerRegion.style.alignItems = Align.Center;
        highScoreBannerRegion.style.marginBottom = 16;
        highScoreBannerRegion.style.display = DisplayStyle.None;

        var highScoreLabel = new Label("NEW HIGH SCORE");
        highScoreLabel.style.fontSize = 32;
        highScoreLabel.style.color = new StyleColor(Color.yellow);
        if (_font != null) highScoreLabel.style.unityFontDefinition = new StyleFontDefinition(_font);
        highScoreBannerRegion.Add(highScoreLabel);

        root.Add(highScoreBannerRegion);

        _promptLabel = new Label("PRESS DOWN TO RETURN");
        _promptLabel.style.fontSize = 18;
        _promptLabel.style.color = new StyleColor(Color.white);
        _promptLabel.style.marginTop = 32;
        if (_font != null) _promptLabel.style.unityFontDefinition = new StyleFontDefinition(_font);
        root.Add(_promptLabel);

        ApplyInitialHidden(); // no boot-frame flash before the state machine's first transition
    }

    private void Update()
    {
        if (!IsVisible) return;
        // Ignore the frame this screen became visible: the Down press that ended the game (and
        // transitioned us here) still reads as wasPressedThisFrame this frame, and would
        // otherwise immediately fire OnReturnPressed and skip the Game Over screen entirely.
        if (Time.frameCount == _shownFrame) return;
        if (Keyboard.current != null && Keyboard.current.downArrowKey.wasPressedThisFrame)
            OnReturnPressed?.Invoke();
    }

    public void ShowWithResult(int finalScore, bool isNewHighScore)
    {
        base.Show();
        _shownFrame = Time.frameCount;
        if (_scoreValueLabel != null)
            _scoreValueLabel.text = finalScore.ToString();
        var highScoreBannerRegion = Root.Q<VisualElement>("highScoreBannerRegion");
        if (highScoreBannerRegion != null)
            highScoreBannerRegion.style.display = isNewHighScore ? DisplayStyle.Flex : DisplayStyle.None;
        if (_blinkCoroutine != null) StopCoroutine(_blinkCoroutine);
        _blinkCoroutine = StartCoroutine(BlinkCoroutine());
    }

    public override void Hide()
    {
        base.Hide();
        if (_blinkCoroutine != null)
        {
            StopCoroutine(_blinkCoroutine);
            _blinkCoroutine = null;
        }
    }

    private IEnumerator BlinkCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.8f);
            if (_promptLabel == null) yield break;
            // Toggle visibility (not display) so the label keeps its layout space and the
            // centered column does not reflow/jump every blink.
            var current = _promptLabel.style.visibility.value;
            _promptLabel.style.visibility = current == Visibility.Hidden ? Visibility.Visible : Visibility.Hidden;
        }
    }
}
