using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using OneTimeGames.CoreSystems;

// Runtime-built on-screen touch controls for mobile web play, wired entirely in code so the
// scene needs no manual setup. Creates a CoreSystems MobileControlsOverlay (which decides its
// own visibility -- shown only on touch devices, via the browser feature-detection .jslib on
// WebGL) plus a left/right/down VirtualDPad and a rotate VirtualButton, and binds them into
// TetrisInputHandler. Desktop keyboard play is unaffected: the overlay stays hidden and the
// bound controls simply report no input.
//
// The UI is authored for landscape; on a touch device held in portrait the controls would be
// oversized/off-screen, so in portrait we hide them and show a "rotate your device" hint instead.
public class MobileControls : MonoBehaviour
{
    private TetrisInputHandler _input;
    private MobileControlsOverlay _overlay;
    private VisualElement _hint;
    private bool _deviceWantsControls;
    private bool _ready;
    private bool _desiredVisible;

    // Spawns the controls object at runtime and returns the handle used to Show()/Hide() it per
    // game state. Safe to call even on desktop -- the overlay hides itself there.
    public static MobileControls Spawn(TetrisInputHandler input)
    {
        var go = new GameObject("MobileControls");
        var mc = go.AddComponent<MobileControls>();
        mc._input = input;
        return mc;
    }

    private IEnumerator Start()
    {
        // Reuse whatever PanelSettings the existing screen UIDocuments already render with, so
        // the overlay matches their scale/resolution without needing a serialized asset ref.
        PanelSettings panelSettings = null;
        foreach (var doc in FindObjectsByType<UIDocument>(FindObjectsSortMode.None))
        {
            if (doc.panelSettings != null) { panelSettings = doc.panelSettings; break; }
        }
        if (panelSettings == null) yield break; // no UI panel to hang the controls on

        var overlayGo = new GameObject("TouchOverlay");
        overlayGo.transform.SetParent(transform, false);
        var doc2 = overlayGo.AddComponent<UIDocument>();
        doc2.panelSettings = panelSettings;
        doc2.sortingOrder = 100; // above the game screens so buttons sit on top and receive taps

        yield return null; // let the UIDocument build its rootVisualElement in OnEnable

        // This overlay renders above the game screens (sortingOrder 100). Its root fills the
        // panel, so if it were pickable it would swallow every tap -- including taps meant for
        // the start screen underneath (breaking tap-to-start). Make the root ignore picking; the
        // actual control buttons are pickable children and are unaffected.
        if (doc2.rootVisualElement != null)
            doc2.rootVisualElement.pickingMode = PickingMode.Ignore;

        // Adding the overlay runs its Awake, which builds the container and decides visibility.
        _overlay = overlayGo.AddComponent<MobileControlsOverlay>();
        _deviceWantsControls = _overlay.Root != null
            && _overlay.Root.style.display.value == DisplayStyle.Flex;
#if UNITY_EDITOR
        _deviceWantsControls = true; // let a developer see/click the controls in Play mode
#endif

        // D-pad (defaults: bottom-left, 4-directional) drives left/right + soft drop.
        var dpad = overlayGo.AddComponent<VirtualDPad>();

        // Rotate button, bottom-right. Empty label -- the "rotate" glyph isn't in the runtime
        // font; a drawn circular-arrow icon is added over it below instead. Configure() before Start().
        var rotateBtn = overlayGo.AddComponent<VirtualButton>();
        rotateBtn.Configure("", new Vector2(70f, 9f), 110f);

        yield return null; // let the controls' Start() build visuals + register with the overlay

        AddRotateIcon(new Vector2(70f, 9f), 110f);
        BuildPortraitHint(doc2);

        if (_input != null) _input.BindVirtualControls(dpad, rotateBtn);

        _ready = true;
        Refresh();
    }

    // Decorative circular-arrow icon drawn as shapes (no font glyph) over the rotate button.
    private void AddRotateIcon(Vector2 screenPosition, float sizePx)
    {
        if (_overlay == null || _overlay.Root == null) return;
        var slot = new VisualElement();
        slot.style.position = Position.Absolute;
        slot.style.left = Length.Percent(screenPosition.x);
        slot.style.top = Length.Percent(100f - screenPosition.y);
        slot.style.marginTop = -sizePx;
        slot.style.width = sizePx;
        slot.style.height = sizePx;
        slot.style.alignItems = Align.Center;
        slot.style.justifyContent = Justify.Center;
        slot.pickingMode = PickingMode.Ignore;

        float ring = sizePx * 0.5f;
        var arrow = new Color(1f, 1f, 1f, 0.95f);
        var clear = new Color(0f, 0f, 0f, 0f);

        var circle = new VisualElement();
        circle.style.width = ring;
        circle.style.height = ring;
        circle.pickingMode = PickingMode.Ignore;
        float bw = ring * 0.16f;
        circle.style.borderTopWidth = bw; circle.style.borderBottomWidth = bw;
        circle.style.borderLeftWidth = bw; circle.style.borderRightWidth = bw;
        // Open the ring at the top-right so it reads as a rotation arrow, not a solid O.
        circle.style.borderBottomColor = arrow; circle.style.borderLeftColor = arrow;
        circle.style.borderRightColor = arrow; circle.style.borderTopColor = clear;
        float r = ring * 0.5f;
        circle.style.borderTopLeftRadius = r; circle.style.borderTopRightRadius = r;
        circle.style.borderBottomLeftRadius = r; circle.style.borderBottomRightRadius = r;
        slot.Add(circle);

        // Arrowhead at the gap (top area), pointing right to imply clockwise rotation.
        var head = new VisualElement();
        head.style.position = Position.Absolute;
        head.style.top = (sizePx - ring) * 0.5f - bw;
        head.style.left = sizePx * 0.5f + ring * 0.2f;
        head.style.width = 0; head.style.height = 0;
        float hh = ring * 0.22f;
        head.style.borderTopWidth = hh; head.style.borderBottomWidth = hh;
        head.style.borderLeftWidth = hh * 1.2f;
        head.style.borderTopColor = clear; head.style.borderBottomColor = clear;
        head.style.borderLeftColor = arrow; head.style.borderRightColor = clear;
        head.pickingMode = PickingMode.Ignore;
        slot.Add(head);

        _overlay.Root.Add(slot);
    }

    // Full-screen hint shown (in place of the controls) when a touch device is held in portrait.
    private void BuildPortraitHint(UIDocument doc)
    {
        var root = doc != null ? doc.rootVisualElement : null;
        if (root == null) return;

        _hint = new VisualElement();
        _hint.style.position = Position.Absolute;
        _hint.style.left = 0; _hint.style.top = 0;
        _hint.style.width = Length.Percent(100);
        _hint.style.height = Length.Percent(100);
        _hint.style.backgroundColor = new Color(0.02f, 0.02f, 0.02f, 1f);
        _hint.style.alignItems = Align.Center;
        _hint.style.justifyContent = Justify.Center;
        _hint.style.paddingLeft = 16; _hint.style.paddingRight = 16;
        _hint.style.display = DisplayStyle.None;
        _hint.pickingMode = PickingMode.Ignore;

        var l1 = new Label("ROTATE YOUR DEVICE");
        l1.style.color = Color.white;
        l1.style.fontSize = 30;
        l1.style.unityFontStyleAndWeight = FontStyle.Bold;
        l1.style.marginBottom = 12;
        l1.style.whiteSpace = WhiteSpace.Normal;
        l1.style.maxWidth = Length.Percent(90);
        l1.style.unityTextAlign = TextAnchor.MiddleCenter;

        var l2 = new Label("TO PLAY IN LANDSCAPE");
        l2.style.color = new Color(1f, 1f, 1f, 0.7f);
        l2.style.fontSize = 20;
        l2.style.whiteSpace = WhiteSpace.Normal;
        l2.style.maxWidth = Length.Percent(90);
        l2.style.unityTextAlign = TextAnchor.MiddleCenter;

        _hint.Add(l1);
        _hint.Add(l2);
        root.Add(_hint);
    }

    public void Show()
    {
        _desiredVisible = true;
        Refresh();
    }

    public void Hide()
    {
        _desiredVisible = false;
        Refresh();
    }

    private void Update()
    {
        // Orientation can change at any time; keep control/hint visibility in sync with it.
        if (_ready) Refresh();
    }

    private void Refresh()
    {
        if (!_ready || _overlay == null || _overlay.Root == null) return;
        bool wantControls = _deviceWantsControls && _desiredVisible;
        bool portrait = Screen.height > Screen.width;
        _overlay.Root.style.display = (wantControls && !portrait) ? DisplayStyle.Flex : DisplayStyle.None;
        if (_hint != null)
            _hint.style.display = (wantControls && portrait) ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
