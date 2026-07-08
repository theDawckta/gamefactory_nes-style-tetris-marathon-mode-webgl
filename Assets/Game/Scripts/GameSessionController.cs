using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OneTimeGames.CoreSystems;

public class GameSessionController : MonoBehaviour
{
    [SerializeField] private StartScreen startScreen;
    [SerializeField] private GameScreen gameScreen;
    [SerializeField] private GameOverScreen gameOverScreen;
    [SerializeField] private PlayfieldController playfieldController;
    [SerializeField] private TetrisInputHandler tetrisInputHandler;
    [SerializeField] private LeaderboardClient leaderboardClient;
    [SerializeField] private FactoryAuthController authController;
    [SerializeField] private LeaderboardWidget leaderboardWidget;

    private GameStateMachine _stateMachine;
    private List<LeaderboardEntry> _cachedTopFive;
    private int _finalScore;
    private MobileControls _mobileControls;

    private IEnumerator Start()
    {
        EnsureCamera();
        _mobileControls = MobileControls.Spawn(tetrisInputHandler);

        if (ConfigService.Instance != null)
            yield return ConfigService.Instance.EnsureLoaded();

        if (authController != null && !authController.IsResolved)
        {
            var resolved = false;
            Action<FactoryAuthController> onResolved = _ => resolved = true;
            authController.OnIdentityResolved += onResolved;
            while (!resolved)
                yield return null;
            authController.OnIdentityResolved -= onResolved;
        }

        SetupStateMachine();
        _stateMachine.TransitionTo("StartScreen");
    }

    // This is a pure UI Toolkit (screen-space overlay) game with no world geometry, so the scene
    // ships without a Camera -- which makes Unity show the "No cameras rendering" overlay and
    // leaves a WebGL build with nothing clearing the screen. Create a minimal clear-only camera
    // at runtime if none exists (renders no objects; the UITK panel draws on top).
    private void EnsureCamera()
    {
        if (Camera.main != null) return;
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.cullingMask = 0;
        cam.orthographic = true;
    }

    private void SetupStateMachine()
    {
        _stateMachine = new GameStateMachine();
        _stateMachine.RegisterState("StartScreen", EnterStartScreen, null, ExitStartScreen);
        _stateMachine.RegisterState("Playing", EnterPlaying, null, ExitPlaying);
        _stateMachine.RegisterState("GameOver", EnterGameOver, null, ExitGameOver);
    }

    private void EnterStartScreen()
    {
        gameScreen?.Hide();
        gameOverScreen?.Hide();
        startScreen?.Show();
        _mobileControls?.Hide();
        if (leaderboardClient != null)
            StartCoroutine(leaderboardClient.FetchTopFive(
                entries => _cachedTopFive = entries,
                _ => { }
            ));
        if (startScreen != null)
            startScreen.OnStartPressed += OnStartPressed;
    }

    private void ExitStartScreen()
    {
        if (startScreen != null)
            startScreen.OnStartPressed -= OnStartPressed;
    }

    private void OnStartPressed() => _stateMachine.TransitionTo("Playing");

    private void EnterPlaying()
    {
        startScreen?.Hide();
        gameOverScreen?.Hide();
        gameScreen?.Show();
        _mobileControls?.Show();
        playfieldController?.StartGame();
        tetrisInputHandler?.Enable();
        if (playfieldController != null)
            playfieldController.OnGameOver += OnGameOver;
    }

    private void ExitPlaying()
    {
        if (playfieldController != null)
            playfieldController.OnGameOver -= OnGameOver;
    }

    private void OnGameOver()
    {
        _finalScore = playfieldController != null ? playfieldController.CurrentScore : 0;
        _stateMachine.TransitionTo("GameOver");
    }

    private void EnterGameOver()
    {
        gameScreen?.Hide();
        _mobileControls?.Hide();
        tetrisInputHandler?.Disable();

        var isNewHighScore = DetermineNewHighScore(_finalScore);

        if (authController != null && !authController.IsGuest && leaderboardClient != null)
            StartCoroutine(SubmitScoreAsync(authController.Username, authController.Token, _finalScore));

        gameOverScreen?.ShowWithResult(_finalScore, isNewHighScore);
        if (gameOverScreen != null)
            gameOverScreen.OnReturnPressed += OnReturnPressed;
    }

    private void ExitGameOver()
    {
        if (gameOverScreen != null)
            gameOverScreen.OnReturnPressed -= OnReturnPressed;
    }

    private void OnReturnPressed() => _stateMachine.TransitionTo("StartScreen");

    private bool DetermineNewHighScore(int score)
    {
        if (authController == null || authController.IsGuest) return false;
        if (_cachedTopFive == null) return false;

        var qualifiesForTopFive = _cachedTopFive.Count < 5
            || score > _cachedTopFive[_cachedTopFive.Count - 1].score;

        if (!qualifiesForTopFive) return false;

        var playerBest = FindPlayerBest();
        return playerBest < 0 || score > playerBest;
    }

    private int FindPlayerBest()
    {
        if (_cachedTopFive == null || authController == null) return -1;
        var username = authController.Username;
        if (string.IsNullOrEmpty(username)) return -1;
        foreach (var entry in _cachedTopFive)
            if (string.Equals(entry.username, username, StringComparison.OrdinalIgnoreCase))
                return entry.score;
        return -1;
    }

    private IEnumerator SubmitScoreAsync(string username, string token, int score)
    {
        List<LeaderboardEntry> updated = null;
        yield return leaderboardClient.SubmitScore(username, token, score,
            entries => updated = entries,
            _ => { }
        );
        if (updated != null)
            _cachedTopFive = updated;
    }

    // QA Navigation Contract
    public void StartGame()
    {
        _stateMachine?.TransitionTo("Playing");
    }

    public void GoToGameOver()
    {
        if (_stateMachine == null) return;
        _finalScore = playfieldController != null ? playfieldController.CurrentScore : 0;
        playfieldController?.StopGame();
        _stateMachine.TransitionTo("GameOver");
    }

    public void GoToStart()
    {
        _stateMachine?.TransitionTo("StartScreen");
    }
}
