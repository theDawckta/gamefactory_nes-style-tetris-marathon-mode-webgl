using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using OneTimeGames.CoreSystems;

// Split-screen gesture controls for mobile web play, wired entirely in code so the scene
// needs no manual setup. Left half of the screen handles movement (swipe left/right/down)
// with NES DAS hold-repeat; right half handles rotation (swipe in either direction = CW rotate).
// Overlay auto-hides on desktop via MobileControlsOverlay browser feature-detection jslib.
// Keyboard input via TetrisInputHandler is unaffected.
public class MobileTetrisInput : MonoBehaviour
{
    [SerializeField] private PlayfieldController _playfieldController;

    private const float DasDelay = 0.170f;
    private const float DasRepeat = 0.050f;

    private GestureZone _leftZone;
    private GestureZone _rightZone;
    private bool _inputEnabled;

    public static MobileTetrisInput Spawn(PlayfieldController playfieldController)
    {
        var go = new GameObject("MobileTetrisInput");
        var comp = go.AddComponent<MobileTetrisInput>();
        comp._playfieldController = playfieldController;
        return comp;
    }

    private IEnumerator Start()
    {
        PanelSettings panelSettings = null;
        foreach (var doc in FindObjectsByType<UIDocument>(FindObjectsSortMode.None))
            if (doc.panelSettings != null) { panelSettings = doc.panelSettings; break; }
        if (panelSettings == null) yield break;

        var overlayGo = new GameObject("MobileTetrisOverlay");
        overlayGo.transform.SetParent(transform, false);
        var doc2 = overlayGo.AddComponent<UIDocument>();
        doc2.panelSettings = panelSettings;
        doc2.sortingOrder = 100;

        yield return null; // let UIDocument build rootVisualElement

        if (doc2.rootVisualElement != null)
            doc2.rootVisualElement.pickingMode = PickingMode.Ignore;
        overlayGo.AddComponent<MobileControlsOverlay>();

        // regionRect is percent (0-100), x/y from bottom-left.
        var leftGo = new GameObject("LeftGestureZone");
        leftGo.transform.SetParent(overlayGo.transform, false);
        _leftZone = leftGo.AddComponent<GestureZone>();
        _leftZone.Configure(new Rect(0f, 0f, 50f, 100f));
        _leftZone.SetHoldRepeatEnabled(true);
        _leftZone.SetHoldRepeat(DasDelay, DasRepeat);

        var rightGo = new GameObject("RightGestureZone");
        rightGo.transform.SetParent(overlayGo.transform, false);
        _rightZone = rightGo.AddComponent<GestureZone>();
        _rightZone.Configure(new Rect(50f, 0f, 50f, 100f));

        yield return null; // let GestureZone Start() register with the overlay

        // Left zone: OnHoldRepeat fires immediately on swipe and again after DasDelay,
        // then every DasRepeat -- gives NES DAS without subscribing to OnSwipe (which
        // would double-fire the first move via StartOrResetRepeat).
        _leftZone.OnHoldRepeat += OnLeftZoneAction;
        // Right zone: one CW rotate per distinct swipe gesture, no auto-repeat.
        _rightZone.OnSwipe += OnRightZoneAction;
    }

    private void OnLeftZoneAction(SwipeDirection dir)
    {
        if (!_inputEnabled || _playfieldController == null) return;
        if (dir == SwipeDirection.Left) _playfieldController.MoveLeft();
        else if (dir == SwipeDirection.Right) _playfieldController.MoveRight();
        else if (dir == SwipeDirection.Down) _playfieldController.SoftDrop();
    }

    private void OnRightZoneAction(SwipeDirection dir)
    {
        if (!_inputEnabled || _playfieldController == null) return;
        if (dir == SwipeDirection.Left || dir == SwipeDirection.Right)
            _playfieldController.Rotate();
    }

    public void Enable() => _inputEnabled = true;
    public void Disable() => _inputEnabled = false;
}
