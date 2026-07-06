using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class TetrisInputHandlerTests : InputTestFixture
{
    private GameObject _go;
    private TetrisInputHandler _handler;
    private Keyboard _keyboard;

    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _keyboard = InputSystem.AddDevice<Keyboard>();
        _go = new GameObject("TetrisInputHandler");
        _handler = _go.AddComponent<TetrisInputHandler>();
    }

    [TearDown]
    public override void TearDown()
    {
        if (_go != null) Object.Destroy(_go);
        base.TearDown();
    }

    [UnityTest]
    public IEnumerator Component_AttachesToGameObject()
    {
        yield return null;
        Assert.IsNotNull(_handler);
    }

    [UnityTest]
    public IEnumerator DefaultValues_AllZeroOrFalse()
    {
        yield return null;
        Assert.AreEqual(0, _handler.MoveDirection);
        Assert.IsFalse(_handler.IsSoftDropping);
        Assert.IsFalse(_handler.RotatePressedThisFrame);
    }

    [UnityTest]
    public IEnumerator LeftPress_MoveDirectionIsMinusOne()
    {
        // Queue event; player loop processes it on next yield
        Press(_keyboard.leftArrowKey);
        yield return null;
        Assert.AreEqual(-1, _handler.MoveDirection);
        Release(_keyboard.leftArrowKey);
        yield return null;
    }

    [UnityTest]
    public IEnumerator RightPress_MoveDirectionIsPlusOne()
    {
        Press(_keyboard.rightArrowKey);
        yield return null;
        Assert.AreEqual(1, _handler.MoveDirection);
        Release(_keyboard.rightArrowKey);
        yield return null;
    }

    [UnityTest]
    public IEnumerator BothLeftAndRight_MoveDirectionIsZero()
    {
        // Use a single full-state event so both bits are set simultaneously.
        // Two separate Press() calls share a byte in KeyboardState and the second
        // delta event would clear the first key's bit.
        InputSystem.QueueStateEvent(_keyboard, new KeyboardState(Key.LeftArrow, Key.RightArrow));
        yield return null;
        Assert.AreEqual(0, _handler.MoveDirection);
        InputSystem.QueueStateEvent(_keyboard, new KeyboardState());
        yield return null;
    }

    [UnityTest]
    public IEnumerator DownPress_IsSoftDroppingTrue()
    {
        Press(_keyboard.downArrowKey);
        yield return null;
        Assert.IsTrue(_handler.IsSoftDropping);
        Release(_keyboard.downArrowKey);
        yield return null;
    }

    [UnityTest]
    public IEnumerator DownRelease_IsSoftDroppingFalse()
    {
        Press(_keyboard.downArrowKey);
        yield return null;
        Release(_keyboard.downArrowKey);
        yield return null;
        Assert.IsFalse(_handler.IsSoftDropping);
    }

    [UnityTest]
    public IEnumerator UpPress_RotatePressedThisFrameTrue()
    {
        Press(_keyboard.upArrowKey);
        yield return null;
        Assert.IsTrue(_handler.RotatePressedThisFrame);
        Release(_keyboard.upArrowKey);
        yield return null;
    }

    [UnityTest]
    public IEnumerator UpHeld_RotatePressedThisFrame_OnlyOnFirstFrame()
    {
        Press(_keyboard.upArrowKey);
        yield return null;
        Assert.IsTrue(_handler.RotatePressedThisFrame);

        // Key stays held -- no Release queued; advance one more frame
        yield return null;
        // wasPressedThisFrame is false on held frames, so handler resets it
        Assert.IsFalse(_handler.RotatePressedThisFrame);

        Release(_keyboard.upArrowKey);
        yield return null;
    }

    [UnityTest]
    public IEnumerator LeftRelease_ResetsMoveDirection()
    {
        Press(_keyboard.leftArrowKey);
        yield return null;
        Assert.AreEqual(-1, _handler.MoveDirection);

        Release(_keyboard.leftArrowKey);
        yield return null;
        Assert.AreEqual(0, _handler.MoveDirection);
    }

    [UnityTest]
    public IEnumerator Disable_SetsAllPropertiesToDefault()
    {
        Press(_keyboard.leftArrowKey);
        Press(_keyboard.downArrowKey);
        yield return null;

        _handler.Disable();
        Assert.AreEqual(0, _handler.MoveDirection);
        Assert.IsFalse(_handler.IsSoftDropping);
        Assert.IsFalse(_handler.RotatePressedThisFrame);

        Release(_keyboard.leftArrowKey);
        Release(_keyboard.downArrowKey);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Disable_BlocksInput()
    {
        _handler.Disable();
        Press(_keyboard.leftArrowKey);
        yield return null;
        Assert.AreEqual(0, _handler.MoveDirection);
        Release(_keyboard.leftArrowKey);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Enable_AfterDisable_RestoresInput()
    {
        _handler.Disable();
        _handler.Enable();
        Press(_keyboard.rightArrowKey);
        yield return null;
        Assert.AreEqual(1, _handler.MoveDirection);
        Release(_keyboard.rightArrowKey);
        yield return null;
    }

    [UnityTest]
    public IEnumerator DAS_HoldingLeft_FiresMultipleTimes()
    {
        int fireCount = 0;
        float elapsed = 0f;

        // Queue press; player loop will process it each frame while held
        Press(_keyboard.leftArrowKey);

        while (elapsed < 0.25f)
        {
            yield return null;
            elapsed += Time.deltaTime;
            if (_handler.MoveDirection == -1)
                fireCount++;
        }

        Release(_keyboard.leftArrowKey);
        yield return null;

        // First press fires immediately + DAS repeats after 170ms at 50ms intervals
        Assert.Greater(fireCount, 1, "DAS should fire multiple times when key held for 250ms");
    }
}
