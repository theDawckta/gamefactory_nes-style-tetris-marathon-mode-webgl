using System;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class TutorialScreen : BaseScreen
{
    [SerializeField] private PlayfieldController _playfieldController;
    [SerializeField] private Texture2D _diagramTexture;
    [SerializeField] private FontAsset _font;

    public event Action OnHide;

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
        root.style.position = Position.Absolute;
        root.style.left = 0;
        root.style.top = 0;
        root.style.right = 0;
        root.style.bottom = 0;
        root.style.backgroundColor = new StyleColor(new Color(0f, 0f, 0f, 0.75f));
        root.style.display = DisplayStyle.None;
        root.style.alignItems = Align.Center;
        root.style.justifyContent = Justify.Center;

        // Full-screen invisible tap target; receives pointer-down outside the container.
        var tapTarget = new VisualElement();
        tapTarget.name = "tapTarget";
        tapTarget.style.position = Position.Absolute;
        tapTarget.style.left = 0;
        tapTarget.style.top = 0;
        tapTarget.style.right = 0;
        tapTarget.style.bottom = 0;
        tapTarget.RegisterCallback<PointerDownEvent>(evt =>
        {
            Dismiss();
            evt.StopPropagation();
        });
        root.Add(tapTarget);

        // Centered dialog container (renders on top of tapTarget -- added after it).
        var container = new VisualElement();
        container.name = "container";
        container.style.flexDirection = FlexDirection.Column;
        container.style.alignItems = Align.Center;
        container.style.position = Position.Relative;
        container.style.backgroundColor = new StyleColor(new Color(0.1f, 0.1f, 0.15f, 0.95f));
        container.style.paddingTop = 12;
        container.style.paddingBottom = 16;
        container.style.paddingLeft = 16;
        container.style.paddingRight = 16;
        container.style.borderTopLeftRadius = 8;
        container.style.borderTopRightRadius = 8;
        container.style.borderBottomLeftRadius = 8;
        container.style.borderBottomRightRadius = 8;
        container.style.maxWidth = 600;
        container.style.width = new StyleLength(new Length(90f, LengthUnit.Percent));
        // Prevent taps on the container body from reaching tapTarget below.
        container.RegisterCallback<PointerDownEvent>(evt => evt.StopPropagation());

        // X close button: top-right corner of the container.
        var closeBtn = new Button(() => Dismiss());
        closeBtn.name = "closeButton";
        closeBtn.text = "X";
        closeBtn.style.position = Position.Absolute;
        closeBtn.style.top = 8;
        closeBtn.style.right = 8;
        closeBtn.style.width = 32;
        closeBtn.style.height = 32;
        closeBtn.style.fontSize = 16;
        closeBtn.style.color = new StyleColor(Color.white);
        closeBtn.style.backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f, 1f));
        closeBtn.style.borderTopLeftRadius = 4;
        closeBtn.style.borderTopRightRadius = 4;
        closeBtn.style.borderBottomLeftRadius = 4;
        closeBtn.style.borderBottomRightRadius = 4;
        if (_font != null) closeBtn.style.unityFontDefinition = new StyleFontDefinition(_font);
        container.Add(closeBtn);

        // Gesture diagram image: max display size 540x270 to stay legible on small phones.
        var diagram = new VisualElement();
        diagram.name = "diagram";
        diagram.style.width = 540;
        diagram.style.maxWidth = new StyleLength(new Length(100f, LengthUnit.Percent));
        diagram.style.height = 270;
        diagram.style.marginTop = 32; // clear the absolutely-positioned close button
        diagram.style.marginBottom = 12;
        if (_diagramTexture != null)
        {
            diagram.style.backgroundImage = new StyleBackground(_diagramTexture);
            diagram.style.backgroundSize = new StyleBackgroundSize(
                new BackgroundSize(BackgroundSizeType.Contain));
        }
        else
        {
            // TODO: replace with custom sprite (tutorial gesture diagram PNG)
            diagram.style.backgroundColor = new StyleColor(new Color(0.2f, 0.3f, 0.5f, 1f));
        }
        container.Add(diagram);

        // Dismiss prompt label.
        var dismissLabel = new Label("Tap anywhere to dismiss");
        dismissLabel.name = "dismissLabel";
        dismissLabel.style.fontSize = 18;
        dismissLabel.style.color = new StyleColor(Color.white);
        dismissLabel.style.marginTop = 8;
        if (_font != null) dismissLabel.style.unityFontDefinition = new StyleFontDefinition(_font);
        container.Add(dismissLabel);

        root.Add(container);
    }

    /// <summary>
    /// Dismiss the tutorial, record that it has been seen, and resume the game.
    /// Protected against cascading input on the frame Show() was called.
    /// </summary>
    public void Dismiss()
    {
        if (Time.frameCount == _shownFrame) return;
        PlayerPrefs.SetInt("tetris_tutorial_seen", 1);
        Hide();
    }

    public override void Show()
    {
        base.Show();
        _shownFrame = Time.frameCount;
        _playfieldController?.Pause();
    }

    public override void Hide()
    {
        base.Hide();
        // OnHide fires first so the first-launch handler in GameSessionController can call
        // StartGame() before Resume() sets _isRunning=true; for mid-game dismissals OnHide
        // has no subscriber from GameSessionController so this is a no-op there.
        OnHide?.Invoke();
        _playfieldController?.Resume();
    }
}
