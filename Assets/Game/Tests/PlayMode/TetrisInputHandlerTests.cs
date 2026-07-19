using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

public class TetrisInputHandlerTests : InputTestFixture
{
    private Keyboard _keyboard;
    private GameObject _go;
    private TetrisInputHandler _handler;

    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _keyboard = InputSystem.AddDevice<Keyboard>();
        _go = new GameObject();
        _handler = _go.AddComponent<TetrisInputHandler>();
    }

    [TearDown]
    public override void TearDown()
    {
        Object.Destroy(_go);
        base.TearDown();
    }

    [UnityTest]
    public IEnumerator Component_AttachesToGameObject()
    {
        yield return null;
        Assert.IsNotNull(_go.GetComponent<TetrisInputHandler>());
    }

    [UnityTest]
    public IEnumerator DefaultValues_AllZeroOrFalse()
    {
        yield return null;
        Assert.AreEqual(0, _handler.MoveDirection);
        Assert.IsFalse(_handler.IsSoftDropping);
        Assert.IsFalse(_handler.RotatePressedThisFrame);
        Assert.IsFalse(_handler.RotateCCWPressedThisFrame);
        Assert.IsFalse(_handler.HardDropPressedThisFrame);
    }

    [UnityTest]
    public IEnumerator LeftPress_MoveDirectionIsMinusOne()
    {
        Press(_keyboard.leftArrowKey);
        yield return null;
        Assert.AreEqual(-1, _handler.MoveDirection);
    }

    [UnityTest]
    public IEnumerator RightPress_MoveDirectionIsPlusOne()
    {
        Press(_keyboard.rightArrowKey);
        yield return null;
        Assert.AreEqual(1, _handler.MoveDirection);
    }

    [UnityTest]
    public IEnumerator BothLeftAndRight_MoveDirectionIsZero()
    {
        // Both keys set in a single state event to avoid delta-event bit collision
        var state = new KeyboardState(Key.LeftArrow, Key.RightArrow);
        InputSystem.QueueStateEvent(_keyboard, state);
        yield return null;
        Assert.AreEqual(0, _handler.MoveDirection);
    }

    [UnityTest]
    public IEnumerator DownPress_IsSoftDroppingTrue()
    {
        Press(_keyboard.downArrowKey);
        yield return null;
        Assert.IsTrue(_handler.IsSoftDropping);
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
    }

    [UnityTest]
    public IEnumerator UpHeld_RotatePressedThisFrame_OnlyOnFirstFrame()
    {
        Press(_keyboard.upArrowKey);
        yield return null;
        bool firstFrame = _handler.RotatePressedThisFrame;
        yield return null;
        bool secondFrame = _handler.RotatePressedThisFrame;
        Assert.IsTrue(firstFrame);
        Assert.IsFalse(secondFrame);
    }

    [UnityTest]
    public IEnumerator ZPress_RotateCCWPressedThisFrameTrue()
    {
        Press(_keyboard.zKey);
        yield return null;
        Assert.IsTrue(_handler.RotateCCWPressedThisFrame);
    }

    [UnityTest]
    public IEnumerator SpacePress_HardDropPressedThisFrameTrue()
    {
        Press(_keyboard.spaceKey);
        yield return null;
        Assert.IsTrue(_handler.HardDropPressedThisFrame);
    }

    [UnityTest]
    public IEnumerator SpaceHeld_HardDropPressedThisFrame_OnlyOnFirstFrame()
    {
        Press(_keyboard.spaceKey);
        yield return null;
        bool firstFrame = _handler.HardDropPressedThisFrame;
        yield return null;
        bool secondFrame = _handler.HardDropPressedThisFrame;
        Assert.IsTrue(firstFrame);
        Assert.IsFalse(secondFrame);
    }

    [UnityTest]
    public IEnumerator LeftRelease_ResetsMoveDirection()
    {
        Press(_keyboard.leftArrowKey);
        yield return null;
        Release(_keyboard.leftArrowKey);
        yield return null;
        Assert.AreEqual(0, _handler.MoveDirection);
    }

    [UnityTest]
    public IEnumerator Disable_SetsAllPropertiesToDefault()
    {
        Press(_keyboard.leftArrowKey);
        yield return null;
        _handler.Disable();
        Assert.AreEqual(0, _handler.MoveDirection);
        Assert.IsFalse(_handler.IsSoftDropping);
        Assert.IsFalse(_handler.RotatePressedThisFrame);
    }

    [UnityTest]
    public IEnumerator Disable_BlocksInput()
    {
        _handler.Disable();
        Press(_keyboard.leftArrowKey);
        yield return null;
        Assert.AreEqual(0, _handler.MoveDirection);
    }

    [UnityTest]
    public IEnumerator Enable_AfterDisable_RestoresInput()
    {
        _handler.Disable();
        _handler.Enable();
        Press(_keyboard.leftArrowKey);
        yield return null;
        Assert.AreEqual(-1, _handler.MoveDirection);
    }

    [UnityTest]
    public IEnumerator DAS_HoldingLeft_FiresMultipleTimes()
    {
        Press(_keyboard.leftArrowKey);

        int fireCount = 0;
        float startRealTime = Time.realtimeSinceStartup;

        // Monitor for 400ms real time: enough for immediate fire + DAS delay (170ms) + 3+ repeats
        while (Time.realtimeSinceStartup - startRealTime < 0.4f)
        {
            yield return null;
            if (_handler.MoveDirection == -1) fireCount++;
        }

        Assert.GreaterOrEqual(fireCount, 3,
            "Should fire immediately, then repeat after 170ms DAS delay at 50ms intervals");
    }
}
