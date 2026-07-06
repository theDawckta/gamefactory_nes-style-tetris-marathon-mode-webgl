using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class StartScreen : BaseScreen
{
    [SerializeField] private FontAsset _font;
    [SerializeField] private LeaderboardWidget _leaderboardWidget;

    public event Action OnStartPressed;

    public VisualElement LeaderboardRegion { get; private set; }

    private Label _promptLabel;
    private Coroutine _blinkCoroutine;

    private void Start()
    {
        BuildUI();
        _leaderboardWidget?.SetLeaderboardRegion(LeaderboardRegion);
    }

    private void BuildUI()
    {
        var root = Root;
        if (root == null) return;

        root.Clear();
        root.style.flexDirection = FlexDirection.Column;
        root.style.alignItems = Align.Center;
        root.style.justifyContent = Justify.Center;
        root.style.backgroundColor = new StyleColor(new Color(0.039f, 0.039f, 0.039f, 1f));
        root.style.width = new StyleLength(new Length(100f, LengthUnit.Percent));
        root.style.height = new StyleLength(new Length(100f, LengthUnit.Percent));

        var title = new Label("TETRIS");
        title.style.fontSize = 72;
        title.style.color = new StyleColor(Color.white);
        title.style.marginBottom = 24;
        if (_font != null) title.style.unityFontDefinition = new StyleFontDefinition(_font);
        root.Add(title);

        _promptLabel = new Label("PRESS DOWN TO START");
        _promptLabel.style.fontSize = 24;
        _promptLabel.style.color = new StyleColor(Color.white);
        _promptLabel.style.marginBottom = 32;
        if (_font != null) _promptLabel.style.unityFontDefinition = new StyleFontDefinition(_font);
        root.Add(_promptLabel);

        LeaderboardRegion = new VisualElement();
        LeaderboardRegion.name = "leaderboardRegion";
        LeaderboardRegion.style.flexDirection = FlexDirection.Column;
        LeaderboardRegion.style.width = new StyleLength(new Length(60f, LengthUnit.Percent));

        for (int i = 0; i < 5; i++)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.justifyContent = Justify.SpaceBetween;
            row.style.paddingTop = 4;
            row.style.paddingBottom = 4;

            var rank = new Label();
            rank.name = "rank";
            rank.style.color = new StyleColor(Color.white);
            if (_font != null) rank.style.unityFontDefinition = new StyleFontDefinition(_font);

            var score = new Label();
            score.name = "score";
            score.style.color = new StyleColor(Color.white);
            if (_font != null) score.style.unityFontDefinition = new StyleFontDefinition(_font);

            var username = new Label();
            username.name = "username";
            username.style.color = new StyleColor(Color.white);
            if (_font != null) username.style.unityFontDefinition = new StyleFontDefinition(_font);

            var charSlot = new VisualElement();
            charSlot.name = "characterSlot";
            charSlot.style.width = 48;
            charSlot.style.height = 48;

            row.Add(rank);
            row.Add(score);
            row.Add(username);
            row.Add(charSlot);
            LeaderboardRegion.Add(row);
        }

        root.Add(LeaderboardRegion);
    }

    private void Update()
    {
        if (!IsVisible) return;
        if (Keyboard.current != null && Keyboard.current.downArrowKey.wasPressedThisFrame)
            OnStartPressed?.Invoke();
    }

    public override void Show()
    {
        base.Show();
        if (_blinkCoroutine != null) StopCoroutine(_blinkCoroutine);
        _blinkCoroutine = StartCoroutine(BlinkCoroutine());
        _leaderboardWidget?.Refresh();
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
            var current = _promptLabel.style.display.value;
            _promptLabel.style.display = current == DisplayStyle.None ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
