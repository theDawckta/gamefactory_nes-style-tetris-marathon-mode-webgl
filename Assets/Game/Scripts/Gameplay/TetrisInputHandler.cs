using UnityEngine;

// Stub: full implementation is provided by issue #5.
// PlayfieldController reads these properties each Update.
public class TetrisInputHandler : MonoBehaviour
{
    public int MoveDirection { get; private set; }
    public bool IsSoftDropping { get; private set; }
    public bool RotatePressedThisFrame { get; private set; }
}
