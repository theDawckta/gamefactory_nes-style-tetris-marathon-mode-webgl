using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using OneTimeGames.CoreSystems;

// Split-screen gesture controls for mobile web play, wired entirely in code so the scene
// needs no manual setup. Left half of the screen handles horizontal movement (swipe
// left/right, NES DAS hold-repeat); right half handles rotation (swipe left/right = CW
// rotate) AND soft drop (swipe down, hold-repeat at DAS cadence).
// Overlay auto-hides on desktop via MobileControlsOverlay browser feature-detection jslib.
// Keyboard input via TetrisInputHandler is unaffected.
public class MobileTetrisInput : MonoBehaviour
{
    [SerializeField] private PlayfieldController _playfieldController;

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
        // Pick the GAME panel deterministically via the GameScreen's own document.
        // "First UIDocument found" is no longer safe: tutorial/help layers live on a
        // CLONED overlay PanelSettings (portrait width-fit exemption), and grabbing one
        // of those here trips Unity's cross-panel assertion in UIDocument.set_panelSettings.
        // The gesture zones belong on the MAIN game panel (they are percent-sized, so
        // they cover the full screen in either orientation).
        PanelSettings panelSettings = null;
        var gameScreen = FindFirstObjectByType<GameScreen>(FindObjectsInactive.Include);
        if (gameScreen != null)
        {
            var gsDoc = gameScreen.GetComponent<UIDocument>();
            if (gsDoc != null) panelSettings = gsDoc.panelSettings;
        }
        if (panelSettings == null)
            foreach (var doc in FindObjectsByType<UIDocument>(FindObjectsInactive.Include))
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
        _leftZone.SetHoldRepeat(TetrisInputHandler.DasDelay, TetrisInputHandler.DasRepeat);

        var rightGo = new GameObject("RightGestureZone");
        rightGo.transform.SetParent(overlayGo.transform, false);
        _rightZone = rightGo.AddComponent<GestureZone>();
        _rightZone.Configure(new Rect(50f, 0f, 50f, 100f));
        _rightZone.SetHoldRepeatEnabled(true);
        _rightZone.SetHoldRepeat(TetrisInputHandler.DasDelay, TetrisInputHandler.DasRepeat);

        yield return null; // let GestureZone Start() register with the overlay

        // Left zone (MOVE only): OnHoldRepeat fires immediately on swipe and again after
        // DasDelay, then every DasRepeat -- gives NES DAS without subscribing to OnSwipe
        // (which would double-fire the first move via StartOrResetRepeat).
        _leftZone.OnHoldRepeat += OnLeftZoneAction;
        // Right zone (ROTATE + SOFT DROP): rotate is one CW rotate per distinct swipe
        // gesture via OnSwipe; soft drop is swipe DOWN handled ONLY in the hold-repeat
        // handler (immediate pulse + DAS cadence while held). The two handlers split by
        // direction so a held horizontal swipe never machine-guns rotation, and Down
        // never double-fires (it is deliberately absent from the OnSwipe handler).
        _rightZone.OnSwipe += OnRightZoneAction;
        _rightZone.OnHoldRepeat += OnRightZoneHoldRepeat;
    }

    private void OnLeftZoneAction(SwipeDirection dir)
    {
        if (!_inputEnabled || _playfieldController == null) return;
        if (dir == SwipeDirection.Left) _playfieldController.MoveLeft();
        else if (dir == SwipeDirection.Right) _playfieldController.MoveRight();
    }

    private void OnRightZoneAction(SwipeDirection dir)
    {
        if (!_inputEnabled || _playfieldController == null) return;
        if (dir == SwipeDirection.Left || dir == SwipeDirection.Right)
            _playfieldController.Rotate();
    }

    private void OnRightZoneHoldRepeat(SwipeDirection dir)
    {
        if (!_inputEnabled || _playfieldController == null) return;
        if (dir == SwipeDirection.Down) _playfieldController.SoftDrop();
    }

    public void Enable() => _inputEnabled = true;
    public void Disable() => _inputEnabled = false;
}
