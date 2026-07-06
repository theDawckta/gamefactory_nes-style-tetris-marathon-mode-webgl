using UnityEngine;
using UnityEngine.InputSystem;

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

    private void Update()
    {
        RotatePressedThisFrame = false;
        MoveDirection = 0;
        IsSoftDropping = false;

        if (!_inputEnabled) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        bool leftHeld = kb.leftArrowKey.isPressed;
        bool rightHeld = kb.rightArrowKey.isPressed;

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

        IsSoftDropping = kb.downArrowKey.isPressed;
        RotatePressedThisFrame = kb.upArrowKey.wasPressedThisFrame;
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
