using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

public class GameSessionControllerTests
{
    private GameObject _sessionGo;
    private GameObject _playfieldGo;
    private GameSessionController _controller;
    private PlayfieldController _playfield;
    private TetrisInputHandler _inputHandler;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        _playfieldGo = new GameObject("Playfield");
        _playfield = _playfieldGo.AddComponent<PlayfieldController>();
        _inputHandler = _playfieldGo.AddComponent<TetrisInputHandler>();

        _sessionGo = new GameObject("GameSessionController");
        _controller = _sessionGo.AddComponent<GameSessionController>();

        // Wire collaborators via reflection before Start() runs
        SetField(_controller, "playfieldController", _playfield);
        SetField(_controller, "tetrisInputHandler", _inputHandler);

        yield return null; // Allow Start() coroutine to run
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Object.Destroy(_sessionGo);
        Object.Destroy(_playfieldGo);
        yield return null;
    }

    [UnityTest]
    public IEnumerator StartsInStartScreenState()
    {
        var state = GetCurrentState(_controller);
        Assert.AreEqual("StartScreen", state);
        yield return null;
    }

    [UnityTest]
    public IEnumerator StartGame_TransitionsToPlayingState()
    {
        _controller.StartGame();
        yield return null;
        Assert.AreEqual("Playing", GetCurrentState(_controller));
    }

    [UnityTest]
    public IEnumerator GoToGameOver_TransitionsToGameOverState()
    {
        _controller.StartGame();
        yield return null;
        _controller.GoToGameOver();
        yield return null;
        Assert.AreEqual("GameOver", GetCurrentState(_controller));
    }

    [UnityTest]
    public IEnumerator GoToStart_FromGameOver_TransitionsToStartScreenState()
    {
        _controller.StartGame();
        yield return null;
        _controller.GoToGameOver();
        yield return null;
        _controller.GoToStart();
        yield return null;
        Assert.AreEqual("StartScreen", GetCurrentState(_controller));
    }

    [UnityTest]
    public IEnumerator GoToStart_FromPlaying_TransitionsToStartScreenState()
    {
        _controller.StartGame();
        yield return null;
        _controller.GoToStart();
        yield return null;
        Assert.AreEqual("StartScreen", GetCurrentState(_controller));
    }

    [UnityTest]
    public IEnumerator FullCycle_StartGame_GoToGameOver_GoToStart_CorrectStates()
    {
        Assert.AreEqual("StartScreen", GetCurrentState(_controller));
        _controller.StartGame();
        yield return null;
        Assert.AreEqual("Playing", GetCurrentState(_controller));
        _controller.GoToGameOver();
        yield return null;
        Assert.AreEqual("GameOver", GetCurrentState(_controller));
        _controller.GoToStart();
        yield return null;
        Assert.AreEqual("StartScreen", GetCurrentState(_controller));
    }

    [UnityTest]
    public IEnumerator PlayingState_EnablesInputHandler()
    {
        // Start with input disabled to establish a baseline
        _inputHandler.Disable();
        Assert.IsFalse(GetInputEnabled(_inputHandler));

        _controller.StartGame();
        yield return null;

        Assert.IsTrue(GetInputEnabled(_inputHandler));
    }

    [UnityTest]
    public IEnumerator GameOverState_DisablesInputHandler()
    {
        _controller.StartGame();
        yield return null;
        Assert.IsTrue(GetInputEnabled(_inputHandler));

        _controller.GoToGameOver();
        yield return null;
        Assert.IsFalse(GetInputEnabled(_inputHandler));
    }

    [UnityTest]
    public IEnumerator PlayingState_StartsPlayfieldController()
    {
        _controller.StartGame();
        yield return null;
        Assert.IsTrue(GetIsRunning(_playfield));
    }

    [UnityTest]
    public IEnumerator GoToGameOver_StopsPlayfieldController()
    {
        _controller.StartGame();
        yield return null;
        Assert.IsTrue(GetIsRunning(_playfield));

        _controller.GoToGameOver();
        yield return null;
        Assert.IsFalse(GetIsRunning(_playfield));
    }

    [UnityTest]
    public IEnumerator StartGame_CalledBeforeSetup_DoesNotThrow()
    {
        var go = new GameObject("Bare");
        var bare = go.AddComponent<GameSessionController>();
        // _stateMachine is null until Start() runs next frame -- should not throw
        Assert.DoesNotThrow(() => bare.StartGame());
        Object.Destroy(go);
        yield return null;
    }

    [UnityTest]
    public IEnumerator GoToGameOver_CalledBeforeSetup_DoesNotThrow()
    {
        var go = new GameObject("Bare");
        var bare = go.AddComponent<GameSessionController>();
        Assert.DoesNotThrow(() => bare.GoToGameOver());
        Object.Destroy(go);
        yield return null;
    }

    [UnityTest]
    public IEnumerator GoToStart_CalledBeforeSetup_DoesNotThrow()
    {
        var go = new GameObject("Bare");
        var bare = go.AddComponent<GameSessionController>();
        Assert.DoesNotThrow(() => bare.GoToStart());
        Object.Destroy(go);
        yield return null;
    }

    [UnityTest]
    public IEnumerator MultipleStartGame_Calls_StayInPlayingState()
    {
        _controller.StartGame();
        yield return null;
        _controller.StartGame();
        yield return null;
        Assert.AreEqual("Playing", GetCurrentState(_controller));
    }

    [UnityTest]
    public IEnumerator DetermineNewHighScore_NullAuth_ReturnsFalse()
    {
        // authController is null (not wired) -- should return false
        var result = (bool)typeof(GameSessionController)
            .GetMethod("DetermineNewHighScore", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(_controller, new object[] { 9999 });
        Assert.IsFalse(result);
        yield return null;
    }

    [UnityTest]
    public IEnumerator DetermineNewHighScore_NullCachedList_WithNullAuth_ReturnsFalse()
    {
        // _cachedTopFive is null and authController is null -- both conditions return false
        SetField(_controller, "_cachedTopFive", null);
        var result = (bool)typeof(GameSessionController)
            .GetMethod("DetermineNewHighScore", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(_controller, new object[] { 5000 });
        Assert.IsFalse(result);
        yield return null;
    }

    // Helper: read GameStateMachine.CurrentState via reflection
    private static string GetCurrentState(GameSessionController controller)
    {
        var sm = typeof(GameSessionController)
            .GetField("_stateMachine", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(controller);
        if (sm == null) return null;
        return sm.GetType()
            .GetProperty("CurrentState")
            .GetValue(sm) as string;
    }

    private static bool GetInputEnabled(TetrisInputHandler handler)
    {
        return (bool)typeof(TetrisInputHandler)
            .GetField("_inputEnabled", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(handler);
    }

    private static bool GetIsRunning(PlayfieldController playfield)
    {
        return (bool)typeof(PlayfieldController)
            .GetField("_isRunning", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(playfield);
    }

    private static void SetField(object target, string name, object value)
    {
        target.GetType()
            .GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(target, value);
    }

    // ── Pause / Resume ──────────────────────────────────────────────────────────

    [UnityTest]
    public IEnumerator Pause_DelegatesToPlayfieldPause()
    {
        _controller.StartGame();
        yield return null;
        Assert.IsTrue(GetIsRunning(_playfield), "Precondition: playfield must be running");
        _controller.Pause();
        Assert.IsFalse(GetIsRunning(_playfield), "Pause() must stop the playfield");
    }

    [UnityTest]
    public IEnumerator Resume_DelegatesToPlayfieldResume()
    {
        _controller.StartGame();
        yield return null;
        _controller.Pause();
        Assert.IsFalse(GetIsRunning(_playfield), "Precondition: playfield must be paused");
        _controller.Resume();
        Assert.IsTrue(GetIsRunning(_playfield), "Resume() must restart the playfield");
    }

    [UnityTest]
    public IEnumerator Pause_BeforeGameStarted_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => _controller.Pause());
        yield return null;
    }

    [UnityTest]
    public IEnumerator Resume_BeforeGameStarted_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => _controller.Resume());
        yield return null;
    }

    // ── First-launch tutorial trigger ───────────────────────────────────────────

    private TutorialScreen CreateTutorialScreen()
    {
        var go = new GameObject("TutorialScreen");
        go.SetActive(false);
        var uiDoc = go.AddComponent<UIDocument>();
        uiDoc.panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
        var screen = go.AddComponent<TutorialScreen>();
        go.SetActive(true);
        return screen;
    }

    [UnityTest]
    public IEnumerator FirstLaunch_ShowsTutorialOnStartGame()
    {
        PlayerPrefs.DeleteKey("tetris_tutorial_seen");
        var ts = CreateTutorialScreen();
        SetField(_controller, "tutorialScreen", ts);

        yield return null; // let tutorial screen Start() run
        _controller.StartGame();
        yield return null;

        Assert.IsTrue(ts.IsVisible, "Tutorial must be visible on first launch");
        Object.Destroy(ts.gameObject);
        PlayerPrefs.DeleteKey("tetris_tutorial_seen");
    }

    [UnityTest]
    public IEnumerator FirstLaunch_DoesNotStartPlayfieldBeforeTutorialDismissed()
    {
        PlayerPrefs.DeleteKey("tetris_tutorial_seen");
        var ts = CreateTutorialScreen();
        SetField(_controller, "tutorialScreen", ts);

        yield return null;
        _controller.StartGame();
        yield return null;

        Assert.IsFalse(GetIsRunning(_playfield),
            "Playfield must not run until the tutorial is dismissed on first launch");
        Object.Destroy(ts.gameObject);
        PlayerPrefs.DeleteKey("tetris_tutorial_seen");
    }

    [UnityTest]
    public IEnumerator FirstLaunchDismiss_StartsPlayfield()
    {
        PlayerPrefs.DeleteKey("tetris_tutorial_seen");
        var ts = CreateTutorialScreen();
        SetField(_controller, "tutorialScreen", ts);

        yield return null;
        _controller.StartGame();
        yield return null; // tutorial visible; game not started

        ts.Dismiss();       // player dismisses tutorial
        yield return null;

        Assert.IsTrue(GetIsRunning(_playfield),
            "Playfield must start after tutorial is dismissed on first launch");
        Object.Destroy(ts.gameObject);
        PlayerPrefs.DeleteKey("tetris_tutorial_seen");
    }

    [UnityTest]
    public IEnumerator FirstLaunchDismiss_EnablesInputHandler()
    {
        PlayerPrefs.DeleteKey("tetris_tutorial_seen");
        var ts = CreateTutorialScreen();
        SetField(_controller, "tutorialScreen", ts);
        // _inputEnabled defaults to true; disable first so we can verify Enable() is called.
        _inputHandler.Disable();

        yield return null;
        _controller.StartGame();
        yield return null;
        Assert.IsFalse(GetInputEnabled(_inputHandler),
            "Input must remain disabled while tutorial is shown (Enable() is deferred)");

        ts.Dismiss();
        yield return null;

        Assert.IsTrue(GetInputEnabled(_inputHandler),
            "Input handler must be enabled after tutorial is dismissed on first launch");
        Object.Destroy(ts.gameObject);
        PlayerPrefs.DeleteKey("tetris_tutorial_seen");
    }

    // The tutorial shows on EVERY game start by design (game-director decision) --
    // a previously-seen flag must NOT suppress it.
    [UnityTest]
    public IEnumerator ReturningPlayer_StillShowsTutorialEveryStart()
    {
        PlayerPrefs.SetInt("tetris_tutorial_seen", 1); // legacy flag must be ignored
        var ts = CreateTutorialScreen();
        SetField(_controller, "tutorialScreen", ts);

        yield return null;
        _controller.StartGame();
        yield return null;

        Assert.IsTrue(ts.IsVisible, "Tutorial must show on every game start, even for a returning player");
        Object.Destroy(ts.gameObject);
        PlayerPrefs.DeleteKey("tetris_tutorial_seen");
    }

    [UnityTest]
    public IEnumerator ReturningPlayer_PlayfieldStillDeferredUntilDismiss()
    {
        PlayerPrefs.SetInt("tetris_tutorial_seen", 1); // legacy flag must be ignored
        var ts = CreateTutorialScreen();
        SetField(_controller, "tutorialScreen", ts);

        yield return null;
        _controller.StartGame();
        yield return null;

        Assert.IsFalse(GetIsRunning(_playfield),
            "Playfield must wait for the tutorial dismiss on every start");
        ts.Dismiss();
        yield return null;
        Assert.IsTrue(GetIsRunning(_playfield),
            "Playfield must start after the tutorial is dismissed");
        Object.Destroy(ts.gameObject);
        PlayerPrefs.DeleteKey("tetris_tutorial_seen");
    }
}
