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
public class MobileControls : MonoBehaviour
{
    private TetrisInputHandler _input;
    private MobileControlsOverlay _overlay;
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

        // Adding the overlay runs its Awake, which builds the container and decides visibility.
        _overlay = overlayGo.AddComponent<MobileControlsOverlay>();
        _deviceWantsControls = _overlay.Root != null
            && _overlay.Root.style.display.value == DisplayStyle.Flex;
#if UNITY_EDITOR
        _deviceWantsControls = true; // let a developer see/click the controls in Play mode
#endif

        // D-pad (defaults: bottom-left, 4-directional) drives left/right + soft drop.
        var dpad = overlayGo.AddComponent<VirtualDPad>();

        // Rotate button, bottom-right. Configure() must run before the button's Start().
        var rotateBtn = overlayGo.AddComponent<VirtualButton>();
        rotateBtn.Configure("↻", new Vector2(70f, 9f), 110f);

        yield return null; // let the controls' Start() build visuals + register with the overlay

        if (_input != null) _input.BindVirtualControls(dpad, rotateBtn);

        _ready = true;
        ApplyVisibility();
    }

    public void Show()
    {
        _desiredVisible = true;
        ApplyVisibility();
    }

    public void Hide()
    {
        _desiredVisible = false;
        ApplyVisibility();
    }

    private void ApplyVisibility()
    {
        if (!_ready || _overlay == null || _overlay.Root == null) return;
        _overlay.Root.style.display = (_deviceWantsControls && _desiredVisible)
            ? DisplayStyle.Flex
            : DisplayStyle.None;
    }
}
