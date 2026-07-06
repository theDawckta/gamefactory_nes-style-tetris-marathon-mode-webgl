using UnityEngine;
using UnityEngine.InputSystem;

public class TetrisInputHandler : MonoBehaviour
{
    public int MoveDirection { get; private set; }
    public bool IsSoftDropping { get; private set; }
    public bool RotatePressedThisFrame { get; private set; }

    private bool _inputEnabled = true;

    private const float DasDelay = 0.170f;
    private const float DasRepeat = 0.050f;

    private float _dasTimer;
    private bool _dasActive;
    private int _dasDirection;

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
        _dasTimer = 0f;
        _dasActive = false;
        _dasDirection = 0;
    }

    void Update()
    {
        RotatePressedThisFrame = false;
        MoveDirection = 0;
        IsSoftDropping = false;

        if (!_inputEnabled) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        IsSoftDropping = kb.downArrowKey.isPressed;

        if (kb.upArrowKey.wasPressedThisFrame)
            RotatePressedThisFrame = true;

        bool leftHeld = kb.leftArrowKey.isPressed;
        bool rightHeld = kb.rightArrowKey.isPressed;

        if (leftHeld && rightHeld)
        {
            ResetDas();
            return;
        }

        int newDir = leftHeld ? -1 : (rightHeld ? 1 : 0);

        if (newDir == 0)
        {
            ResetDas();
            return;
        }

        if (newDir != _dasDirection)
        {
            _dasDirection = newDir;
            _dasTimer = 0f;
            _dasActive = false;
            MoveDirection = newDir;
            return;
        }

        _dasTimer += Time.deltaTime;
        if (!_dasActive)
        {
            if (_dasTimer >= DasDelay)
            {
                _dasActive = true;
                _dasTimer = 0f;
                MoveDirection = _dasDirection;
            }
        }
        else
        {
            if (_dasTimer >= DasRepeat)
            {
                _dasTimer -= DasRepeat;
                MoveDirection = _dasDirection;
            }
        }
    }

    private void ResetDas()
    {
        _dasTimer = 0f;
        _dasActive = false;
        _dasDirection = 0;
    }
}
