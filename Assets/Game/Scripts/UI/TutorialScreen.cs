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

    private void Awake()
    {
        // UIDocument layering ladder (factory convention): game screens 0, mobile gesture
        // overlay 100, modal overlays 200, HUD buttons 300. At the scene-authored default
        // of 0 this screen rendered invisibly BEHIND the game UI and the full-screen
        // GestureZones (overlay at 100) swallowed its dismiss taps -- with StartGame()
        // deferred until dismissal, the game deadlocked on first launch on a phone.
        var doc = GetComponent<UIDocument>();
        // Overlay UI must NOT shrink with the portrait width-fit (see OverlayPanelHost) --
        // on the main panel this modal rendered microscopic in portrait.
        doc.panelSettings = OverlayPanelHost.GetOrCreate(doc.panelSettings);
        doc.sortingOrder = 200;
    }

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

        // "Tap ANYWHERE to dismiss" means anywhere -- including on the dialog itself.
        // A single bubble-phase handler on the root sees every pointer-down in the
        // subtree (dialog, diagram, X button, backdrop); no separate tap target and no
        // StopPropagation on the container (which previously made dialog taps dead).
        root.RegisterCallback<PointerDownEvent>(_ => DismissFromPointer());

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

        // No close button: the whole overlay is a tap-to-dismiss target (see the
        // root PointerDownEvent handler above and the "Tap anywhere to dismiss" prompt),
        // so a separate X control was redundant and contradicted the instruction.

        // Gesture legend: a texture can be assigned in the Inspector, but by default the
        // legend is built as two text columns (BuildGestureDiagram) -- crisp at any panel
        // scale and no art-asset dependency (the design PNG never materialized).
        var diagram = new VisualElement();
        diagram.name = "diagram";
        diagram.style.width = new StyleLength(new Length(100f, LengthUnit.Percent));
        diagram.style.height = 200;
        diagram.style.marginTop = 8;
        diagram.style.marginBottom = 12;
        if (_diagramTexture != null)
        {
            diagram.style.backgroundImage = new StyleBackground(_diagramTexture);
            diagram.style.backgroundSize = new StyleBackgroundSize(
                new BackgroundSize(BackgroundSizeType.Contain));
        }
        else
        {
            diagram.Add(BuildGestureDiagram());
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
    /// Dismiss the tutorial and resume the game. (The tutorial shows on EVERY game
    /// start by design -- no "seen" flag is recorded.)
    /// Protected against cascading input on the frame Show() was called.
    /// </summary>
    public void Dismiss()
    {
        if (!IsVisible) return;
        if (Time.frameCount == _shownFrame) return;
        Hide();
    }

    // Pointer-driven dismissal (tap target / X button) additionally ignores a short
    // real-time window after Show(): mobile browsers synthesize a duplicate mouse event
    // shortly AFTER a touch on WebGL, so the same '?' tap that opened the tutorial
    // re-fires as a ghost mouse-down a few frames later and would instantly dismiss it.
    // The window applies only to pointer input, never to the programmatic Dismiss().
    private void DismissFromPointer()
    {
        if (Time.unscaledTime - _shownTime < DismissGuardSeconds) return;
        Dismiss();
    }

    private const float DismissGuardSeconds = 0.4f;
    private float _shownTime = -999f;

    public override void Show()
    {
        base.Show();
        _shownFrame = Time.frameCount;
        _shownTime = Time.unscaledTime;
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

    // --- Gesture legend (text only) ------------------------------------------------
    // Two text columns split by a vertical divider, mirroring the LEFT / RIGHT halves of the
    // screen the player taps. Each column is a yellow header at the TOP with its gesture lines
    // beneath. Arrow graphics were removed on purpose: they cramped the panel, the text names
    // every gesture, and the default font has no arrow glyphs anyway.

    private VisualElement BuildGestureDiagram()
    {
        var row = new VisualElement();
        row.name = "diagramRow";
        row.style.flexDirection = FlexDirection.Row;
        row.style.width = new StyleLength(new Length(100f, LengthUnit.Percent));
        row.style.height = new StyleLength(new Length(100f, LengthUnit.Percent));

        // LEFT half = movement. Wording matches the real gestures (MobileTetrisInput left zone:
        // swipe left/right moves, holding repeats it).
        row.Add(BuildDiagramColumn(
            "LEFT SIDE",
            "SWIPE LEFT/RIGHT = MOVE",
            "SWIPE LEFT/RIGHT + HOLD = KEEP MOVING"));

        var divider = new VisualElement();
        divider.style.width = 2;
        divider.style.backgroundColor = new StyleColor(new Color(1f, 1f, 1f, 0.25f));
        divider.style.marginLeft = 10;
        divider.style.marginRight = 10;
        row.Add(divider);

        // RIGHT half = rotate + drop (MobileTetrisInput right zone: swipe left/right rotates,
        // swipe up hard-drops, swipe-down-and-hold soft-drops).
        row.Add(BuildDiagramColumn(
            "RIGHT SIDE",
            "SWIPE LEFT/RIGHT = ROTATE",
            "SWIPE DOWN + HOLD = SOFT DROP",
            "SWIPE UP = INSTANT DROP"));
        return row;
    }

    // A column: yellow header at the TOP, then the gesture instruction lines beneath it.
    private VisualElement BuildDiagramColumn(params string[] lines)
    {
        var col = new VisualElement();
        col.style.flexGrow = 1;
        col.style.flexBasis = 0;
        // min-width:0 lets the column shrink below its content's intrinsic width; without it a
        // flex item defaults to min-width:auto and a long non-wrapping label pushes the row off
        // the screen edge (the original phone overflow bug).
        col.style.minWidth = 0;
        col.style.alignItems = Align.Center;
        col.style.justifyContent = Justify.FlexStart;

        for (int i = 0; i < lines.Length; i++)
        {
            bool isHeader = i == 0;
            var label = new Label(lines[i]);
            label.style.fontSize = isHeader ? 15 : 11;
            label.style.color = isHeader ? new StyleColor(Color.yellow) : new StyleColor(Color.white);
            label.style.marginTop = isHeader ? 0 : (i == 1 ? 14 : 10);
            // Wrap long instructions and center them within the (narrow) column instead of
            // letting them run off the panel.
            label.style.whiteSpace = WhiteSpace.Normal;
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            label.style.width = new StyleLength(new Length(100f, LengthUnit.Percent));
            if (_font != null)
                label.style.unityFontDefinition = new StyleFontDefinition(_font);
            col.Add(label);
        }
        return col;
    }
}
