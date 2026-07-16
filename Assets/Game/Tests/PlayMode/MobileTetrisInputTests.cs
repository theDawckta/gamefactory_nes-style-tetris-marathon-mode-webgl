using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using OneTimeGames.CoreSystems;

public class MobileTetrisInputTests
{
    private GameObject _playfieldGo;
    private PlayfieldController _playfield;
    private MobileTetrisInput _input;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        _playfieldGo = new GameObject("Playfield");
        _playfield = _playfieldGo.AddComponent<PlayfieldController>();

        // Spawn creates its own GO; no UIDocument exists in test so Start() yields early
        _input = MobileTetrisInput.Spawn(_playfield);
        yield return null;

        _playfield.StartGame();
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Object.Destroy(_input.gameObject);
        Object.Destroy(_playfieldGo);
        yield return null;
    }

    // Calls a private method via reflection -- avoids coupling tests to event wiring
    // (which requires a live UIDocument) while still covering the dispatch logic.
    private static void InvokeLeft(MobileTetrisInput target, SwipeDirection dir)
    {
        typeof(MobileTetrisInput)
            .GetMethod("OnLeftZoneAction", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(target, new object[] { dir });
    }

    private static void InvokeRight(MobileTetrisInput target, SwipeDirection dir)
    {
        typeof(MobileTetrisInput)
            .GetMethod("OnRightZoneAction", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(target, new object[] { dir });
    }

    private static void InvokeRightHold(MobileTetrisInput target, SwipeDirection dir)
    {
        typeof(MobileTetrisInput)
            .GetMethod("OnRightZoneHoldRepeat", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(target, new object[] { dir });
    }

    [UnityTest]
    public IEnumerator Spawn_CreatesMobileTetrisInputComponent()
    {
        Assert.IsNotNull(_input);
        Assert.IsNotNull(_input.GetComponent<MobileTetrisInput>());
        yield return null;
    }

    [UnityTest]
    public IEnumerator Enable_SetsInputEnabled()
    {
        _input.Enable();
        var enabled = (bool)typeof(MobileTetrisInput)
            .GetField("_inputEnabled", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(_input);
        Assert.IsTrue(enabled);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Disable_ClearsInputEnabled()
    {
        _input.Enable();
        _input.Disable();
        var enabled = (bool)typeof(MobileTetrisInput)
            .GetField("_inputEnabled", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(_input);
        Assert.IsFalse(enabled);
        yield return null;
    }

    [UnityTest]
    public IEnumerator OnLeftZoneAction_Left_WhenEnabled_MovesPieceLeft()
    {
        _input.Enable();
        var startX = _playfield.CurrentPiecePosition.x;
        InvokeLeft(_input, SwipeDirection.Left);
        Assert.AreEqual(startX - 1, _playfield.CurrentPiecePosition.x);
        yield return null;
    }

    [UnityTest]
    public IEnumerator OnLeftZoneAction_Right_WhenEnabled_MovesPieceRight()
    {
        _input.Enable();
        var startX = _playfield.CurrentPiecePosition.x;
        InvokeLeft(_input, SwipeDirection.Right);
        Assert.AreEqual(startX + 1, _playfield.CurrentPiecePosition.x);
        yield return null;
    }

    [UnityTest]
    public IEnumerator OnLeftZoneAction_Down_DoesNothing_DropLivesOnRight()
    {
        // Soft drop moved to the RIGHT zone (game-director decision) -- left is move-only.
        _input.Enable();
        var startY = _playfield.CurrentPiecePosition.y;
        InvokeLeft(_input, SwipeDirection.Down);
        Assert.AreEqual(startY, _playfield.CurrentPiecePosition.y);
        yield return null;
    }

    [UnityTest]
    public IEnumerator OnRightZoneHoldRepeat_Down_WhenEnabled_MovesPieceDown()
    {
        _input.Enable();
        var startY = _playfield.CurrentPiecePosition.y;
        InvokeRightHold(_input, SwipeDirection.Down);
        Assert.AreEqual(startY - 1, _playfield.CurrentPiecePosition.y);
        yield return null;
    }

    [UnityTest]
    public IEnumerator OnRightZoneHoldRepeat_Horizontal_DoesNotRotateOrMove()
    {
        // A held horizontal swipe on the right must not machine-gun rotation -- rotate
        // fires only from the discrete OnSwipe handler.
        _input.Enable();
        var startRot = _playfield.CurrentPieceRotation;
        var startX = _playfield.CurrentPiecePosition.x;
        InvokeRightHold(_input, SwipeDirection.Left);
        InvokeRightHold(_input, SwipeDirection.Right);
        Assert.AreEqual(startRot, _playfield.CurrentPieceRotation);
        Assert.AreEqual(startX, _playfield.CurrentPiecePosition.x);
        yield return null;
    }

    [UnityTest]
    public IEnumerator OnRightZoneAction_Down_DoesNotRotate()
    {
        // Down is handled only by the hold-repeat handler; the swipe handler must not
        // treat it as a rotate (and must not double-fire the drop).
        _input.Enable();
        var startRot = _playfield.CurrentPieceRotation;
        var startY = _playfield.CurrentPiecePosition.y;
        InvokeRight(_input, SwipeDirection.Down);
        Assert.AreEqual(startRot, _playfield.CurrentPieceRotation);
        Assert.AreEqual(startY, _playfield.CurrentPiecePosition.y);
        yield return null;
    }

    [UnityTest]
    public IEnumerator OnRightZoneAction_Left_WhenEnabled_RotatesPiece()
    {
        _input.Enable();
        var startRot = _playfield.CurrentPieceRotation;
        InvokeRight(_input, SwipeDirection.Left);
        Assert.AreEqual((startRot + 1) & 3, _playfield.CurrentPieceRotation);
        yield return null;
    }

    [UnityTest]
    public IEnumerator OnRightZoneAction_Right_WhenEnabled_RotatesPiece()
    {
        _input.Enable();
        var startRot = _playfield.CurrentPieceRotation;
        InvokeRight(_input, SwipeDirection.Right);
        Assert.AreEqual((startRot + 1) & 3, _playfield.CurrentPieceRotation);
        yield return null;
    }

    [UnityTest]
    public IEnumerator OnLeftZoneAction_WhenDisabled_DoesNotMovePiece()
    {
        _input.Disable();
        var startX = _playfield.CurrentPiecePosition.x;
        InvokeLeft(_input, SwipeDirection.Left);
        Assert.AreEqual(startX, _playfield.CurrentPiecePosition.x);
        yield return null;
    }

    [UnityTest]
    public IEnumerator OnRightZoneAction_WhenDisabled_DoesNotRotatePiece()
    {
        _input.Disable();
        var startRot = _playfield.CurrentPieceRotation;
        InvokeRight(_input, SwipeDirection.Left);
        Assert.AreEqual(startRot, _playfield.CurrentPieceRotation);
        yield return null;
    }
}

public class PlayfieldControllerActionTests
{
    private GameObject _go;
    private PlayfieldController _controller;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        _go = new GameObject();
        _controller = _go.AddComponent<PlayfieldController>();
        yield return null;
        _controller.StartGame();
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Object.Destroy(_go);
        yield return null;
    }

    [UnityTest]
    public IEnumerator MoveLeft_WhenRunning_DecrementsX()
    {
        var startX = _controller.CurrentPiecePosition.x;
        _controller.MoveLeft();
        Assert.AreEqual(startX - 1, _controller.CurrentPiecePosition.x);
        yield return null;
    }

    [UnityTest]
    public IEnumerator MoveRight_WhenRunning_IncrementsX()
    {
        var startX = _controller.CurrentPiecePosition.x;
        _controller.MoveRight();
        Assert.AreEqual(startX + 1, _controller.CurrentPiecePosition.x);
        yield return null;
    }

    [UnityTest]
    public IEnumerator SoftDrop_WhenRunning_DecrementsY()
    {
        var startY = _controller.CurrentPiecePosition.y;
        _controller.SoftDrop();
        Assert.AreEqual(startY - 1, _controller.CurrentPiecePosition.y);
        yield return null;
    }

    [UnityTest]
    public IEnumerator SoftDrop_ResetsGravityAccum()
    {
        var field = typeof(PlayfieldController).GetField("_gravityAccum",
            BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(_controller, 0.5f);
        _controller.SoftDrop();
        Assert.AreEqual(0f, (float)field.GetValue(_controller));
        yield return null;
    }

    [UnityTest]
    public IEnumerator Rotate_WhenRunning_IncrementsCWRotation()
    {
        var startRot = _controller.CurrentPieceRotation;
        _controller.Rotate();
        Assert.AreEqual((startRot + 1) & 3, _controller.CurrentPieceRotation);
        yield return null;
    }

    [UnityTest]
    public IEnumerator MoveLeft_WhenNotRunning_DoesNotMove()
    {
        _controller.StopGame();
        var startX = _controller.CurrentPiecePosition.x;
        _controller.MoveLeft();
        Assert.AreEqual(startX, _controller.CurrentPiecePosition.x);
        yield return null;
    }

    [UnityTest]
    public IEnumerator MoveRight_WhenNotRunning_DoesNotMove()
    {
        _controller.StopGame();
        var startX = _controller.CurrentPiecePosition.x;
        _controller.MoveRight();
        Assert.AreEqual(startX, _controller.CurrentPiecePosition.x);
        yield return null;
    }

    [UnityTest]
    public IEnumerator SoftDrop_WhenNotRunning_DoesNotMove()
    {
        _controller.StopGame();
        var startY = _controller.CurrentPiecePosition.y;
        _controller.SoftDrop();
        Assert.AreEqual(startY, _controller.CurrentPiecePosition.y);
        yield return null;
    }

    [UnityTest]
    public IEnumerator Rotate_WhenNotRunning_DoesNotRotate()
    {
        _controller.StopGame();
        var startRot = _controller.CurrentPieceRotation;
        _controller.Rotate();
        Assert.AreEqual(startRot, _controller.CurrentPieceRotation);
        yield return null;
    }
}
