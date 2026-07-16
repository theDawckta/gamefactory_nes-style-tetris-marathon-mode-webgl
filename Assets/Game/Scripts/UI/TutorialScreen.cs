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
            DismissFromPointer();
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
        var closeBtn = new Button(() => DismissFromPointer());
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

        // Gesture diagram: a texture can be assigned in the Inspector, but by default the
        // diagram is DRAWN procedurally (Painter2D arrows + rotate arc + labels) -- crisp at
        // any panel scale and no art-asset dependency (the design PNG never materialized).
        var diagram = new VisualElement();
        diagram.name = "diagram";
        diagram.style.width = new StyleLength(new Length(100f, LengthUnit.Percent));
        diagram.style.height = 200;
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
    /// Dismiss the tutorial, record that it has been seen, and resume the game.
    /// Protected against cascading input on the frame Show() was called.
    /// </summary>
    public void Dismiss()
    {
        if (Time.frameCount == _shownFrame) return;
        PlayerPrefs.SetInt("tetris_tutorial_seen", 1);
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

    // --- Procedural gesture diagram (Painter2D) ------------------------------------
    // Two columns: LEFT half of the screen = D-pad-style move arrows (+down = drop,
    // hold repeats), RIGHT half = swipe-to-rotate arc. Drawn as vector shapes so it is
    // crisp at any panel scale; the default font has no arrow glyphs.

    private VisualElement BuildGestureDiagram()
    {
        var row = new VisualElement();
        row.name = "diagramRow";
        row.style.flexDirection = FlexDirection.Row;
        row.style.width = new StyleLength(new Length(100f, LengthUnit.Percent));
        row.style.height = new StyleLength(new Length(100f, LengthUnit.Percent));

        row.Add(BuildDiagramColumn(new MoveArrowsElement(),
            "LEFT SIDE", "TAP = MOVE / DROP", "HOLD = KEEP MOVING"));

        var divider = new VisualElement();
        divider.style.width = 2;
        divider.style.backgroundColor = new StyleColor(new Color(1f, 1f, 1f, 0.25f));
        divider.style.marginLeft = 6;
        divider.style.marginRight = 6;
        row.Add(divider);

        row.Add(BuildDiagramColumn(new RotateArcElement(),
            "RIGHT SIDE", "SWIPE LEFT / RIGHT", "= ROTATE"));
        return row;
    }

    private VisualElement BuildDiagramColumn(VisualElement art, params string[] lines)
    {
        var col = new VisualElement();
        col.style.flexGrow = 1;
        col.style.flexBasis = 0;
        col.style.alignItems = Align.Center;
        col.style.justifyContent = Justify.FlexStart;

        art.style.width = new StyleLength(new Length(100f, LengthUnit.Percent));
        art.style.height = 100;
        art.style.marginBottom = 6;
        col.Add(art);

        for (int i = 0; i < lines.Length; i++)
        {
            var label = new Label(lines[i]);
            label.style.fontSize = i == 0 ? 14 : 11;
            label.style.color = i == 0
                ? new StyleColor(Color.yellow)
                : new StyleColor(Color.white);
            label.style.marginTop = i == 0 ? 0 : 3;
            if (_font != null)
                label.style.unityFontDefinition = new StyleFontDefinition(_font);
            col.Add(label);
        }
        return col;
    }

    private static void DrawArrow(Painter2D p, Vector2 from, Vector2 to, float thickness)
    {
        var dir = (to - from).normalized;
        var n = new Vector2(-dir.y, dir.x);
        float head = thickness * 2.2f;
        var shaftEnd = to - dir * (head * 1.4f);
        p.fillColor = Color.white;
        p.BeginPath();
        p.MoveTo(from + n * (thickness * 0.5f));
        p.LineTo(shaftEnd + n * (thickness * 0.5f));
        p.LineTo(shaftEnd + n * head);
        p.LineTo(to);
        p.LineTo(shaftEnd - n * head);
        p.LineTo(shaftEnd - n * (thickness * 0.5f));
        p.LineTo(from - n * (thickness * 0.5f));
        p.ClosePath();
        p.Fill();
    }

    // Left column art: left/right move arrows + a down (soft-drop) arrow.
    private class MoveArrowsElement : VisualElement
    {
        public MoveArrowsElement() { generateVisualContent += Draw; }

        private static void Draw(MeshGenerationContext ctx)
        {
            var r = ctx.visualElement.contentRect;
            if (r.width <= 10f || r.height <= 10f) return;
            var p = ctx.painter2D;
            float th = Mathf.Max(4f, r.height * 0.07f);
            float midY = r.height * 0.35f;
            float cx = r.width * 0.5f;
            float len = Mathf.Min(r.width * 0.30f, r.height * 0.55f);
            DrawArrow(p, new Vector2(cx - 10f, midY), new Vector2(cx - 10f - len, midY), th);
            DrawArrow(p, new Vector2(cx + 10f, midY), new Vector2(cx + 10f + len, midY), th);
            DrawArrow(p, new Vector2(cx, r.height * 0.50f), new Vector2(cx, r.height * 0.95f), th);
        }
    }

    // Right column art: a clockwise arc with an arrowhead (swipe-to-rotate).
    private class RotateArcElement : VisualElement
    {
        public RotateArcElement() { generateVisualContent += Draw; }

        private static void Draw(MeshGenerationContext ctx)
        {
            var r = ctx.visualElement.contentRect;
            if (r.width <= 10f || r.height <= 10f) return;
            var p = ctx.painter2D;
            var center = new Vector2(r.width * 0.5f, r.height * 0.5f);
            float radius = Mathf.Min(r.width, r.height) * 0.34f;
            float th = Mathf.Max(4f, r.height * 0.07f);

            p.strokeColor = Color.white;
            p.lineWidth = th;
            p.lineCap = LineCap.Round;
            p.BeginPath();
            p.Arc(center, radius, 60f, 330f); // clockwise sweep, gap at the upper right
            p.Stroke();

            // Arrowhead at the arc's end, pointing along the clockwise tangent.
            float end = 330f * Mathf.Deg2Rad;
            var tip = center + new Vector2(Mathf.Cos(end), Mathf.Sin(end)) * radius;
            var tangent = new Vector2(-Mathf.Sin(end), Mathf.Cos(end));
            var n = new Vector2(-tangent.y, tangent.x);
            float head = th * 2.2f;
            p.fillColor = Color.white;
            p.BeginPath();
            p.MoveTo(tip + tangent * head * 1.6f);
            p.LineTo(tip + n * head);
            p.LineTo(tip - n * head);
            p.ClosePath();
            p.Fill();
        }
    }
}
