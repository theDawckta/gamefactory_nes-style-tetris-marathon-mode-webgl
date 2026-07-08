using UnityEngine;
using UnityEngine.InputSystem;
using OneTimeGames.CoreSystems;

public class TetrisInputHandler : MonoBehaviour
{
    private const float DasDelay = 0.170f;
    private const float DasRepeat = 0.050f;

    public int MoveDirection { get; private set; }
    public bool IsSoftDropping { get; private set; }
    public bool RotatePressedThisFrame { get; private set; }

    private bool _inputEnabled = true;
    private int _dasDirection;
    private float _dasTimer;
    private bool _dasFired;

    // Optional on-screen touch controls (mobile web). Left/right/down come from the D-pad's
    // held direction; rotate is a single tap queued via the button's OnPressed event and
    // consumed on the next frame. All null on desktop, where only the keyboard drives input.
    private VirtualDPad _vDpad;
    private VirtualButton _vRotate;
    private bool _vRotateQueued;

    // Called by MobileControls once the runtime touch controls are built.
    public void BindVirtualControls(VirtualDPad dpad, VirtualButton rotate)
    {
        _vDpad = dpad;
        if (_vRotate != null) _vRotate.OnPressed -= OnVirtualRotate;
        _vRotate = rotate;
        if (_vRotate != null) _vRotate.OnPressed += OnVirtualRotate;
    }

    private void OnVirtualRotate() => _vRotateQueued = true;

    private void Update()
    {
        RotatePressedThisFrame = false;
        MoveDirection = 0;
        IsSoftDropping = false;

        if (!_inputEnabled)
        {
            _vRotateQueued = false;
            return;
        }

        // Keyboard.current is null on a real phone (no physical keyboard), so this must stay
        // null-safe and fall through to the virtual controls -- never early-return on null kb.
        var kb = Keyboard.current;
        int dpadX = _vDpad != null ? _vDpad.Direction.x : 0;
        int dpadY = _vDpad != null ? _vDpad.Direction.y : 0;

        bool leftHeld = (kb != null && kb.leftArrowKey.isPressed) || dpadX < 0;
        bool rightHeld = (kb != null && kb.rightArrowKey.isPressed) || dpadX > 0;

        if (leftHeld && rightHeld)
        {
            _dasDirection = 0;
            _dasTimer = 0f;
            _dasFired = false;
        }
        else if (leftHeld)
        {
            UpdateDas(-1);
        }
        else if (rightHeld)
        {
            UpdateDas(1);
        }
        else
        {
            _dasDirection = 0;
            _dasTimer = 0f;
            _dasFired = false;
        }

        IsSoftDropping = (kb != null && kb.downArrowKey.isPressed) || dpadY < 0;
        RotatePressedThisFrame = (kb != null && kb.upArrowKey.wasPressedThisFrame) || _vRotateQueued;
        _vRotateQueued = false;
    }

    private void UpdateDas(int dir)
    {
        if (_dasDirection != dir)
        {
            _dasDirection = dir;
            _dasTimer = 0f;
            _dasFired = false;
            MoveDirection = dir;
            return;
        }

        _dasTimer += Time.deltaTime;

        if (!_dasFired)
        {
            if (_dasTimer >= DasDelay)
            {
                _dasFired = true;
                _dasTimer -= DasDelay;
                MoveDirection = dir;
            }
        }
        else
        {
            if (_dasTimer >= DasRepeat)
            {
                _dasTimer -= DasRepeat;
                MoveDirection = dir;
            }
        }
    }

    public void Enable()
    {
        _inputEnabled = true;
    }

    public void Disable()
    {
        _inputEnabled = false;
        MoveDirection = 0;
        IsSoftDropping = false;
        RotatePressedThisFrame = false;
        _vRotateQueued = false;
        _dasDirection = 0;
        _dasTimer = 0f;
        _dasFired = false;
    }
}
