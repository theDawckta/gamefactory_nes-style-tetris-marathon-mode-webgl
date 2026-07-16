using UnityEngine;
using UnityEngine.InputSystem;

public class TetrisInputHandler : MonoBehaviour
{
    // Public so MobileTetrisInput can match these values for GestureZone hold-repeat timing.
    public const float DasDelay = 0.170f;
    public const float DasRepeat = 0.050f;

    public int MoveDirection { get; private set; }
    public bool IsSoftDropping { get; private set; }
    public bool RotatePressedThisFrame { get; private set; }

    private bool _inputEnabled = true;
    private int _dasDirection;
    private float _dasTimer;
    private bool _dasFired;

    private void Update()
    {
        RotatePressedThisFrame = false;
        MoveDirection = 0;
        IsSoftDropping = false;

        if (!_inputEnabled) return;

        // Keyboard.current is null on a real phone (no physical keyboard) -- null-safe.
        var kb = Keyboard.current;

        bool leftHeld = kb != null && kb.leftArrowKey.isPressed;
        bool rightHeld = kb != null && kb.rightArrowKey.isPressed;

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

        IsSoftDropping = kb != null && kb.downArrowKey.isPressed;
        RotatePressedThisFrame = kb != null && kb.upArrowKey.wasPressedThisFrame;
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
        _dasDirection = 0;
        _dasTimer = 0f;
        _dasFired = false;
    }
}
