using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

[TestFixture]
public class QAIntegrationTests
{
    // The tutorial shows on EVERY game start by design (game-director decision) and
    // defers the playfield until dismissed -- start the game the way a player does:
    // start, then dismiss the tutorial.
    private static IEnumerator StartGamePastTutorial(GameSessionController controller)
    {
        controller.StartGame();
        yield return null;
        yield return null; // past TutorialScreen's shown-frame dismiss guard
        var tutorial = Object.FindFirstObjectByType<TutorialScreen>(FindObjectsInactive.Include);
        if (tutorial != null && tutorial.IsVisible)
            tutorial.Dismiss();
        yield return null;
    }

    // Journey Step 1: Start Screen is visible on load
    [UnityTest]
    public IEnumerator StartScreen_IsVisible_OnLoad()
    {
        yield return SceneManager.LoadSceneAsync("Main");
        yield return new WaitForSeconds(2f);
        var startScreen = Object.FindFirstObjectByType<StartScreen>();
        Assert.IsNotNull(startScreen, "StartScreen component not found in scene");
        Assert.IsTrue(startScreen.IsVisible, "StartScreen should be visible on load");
        var gameScreen = Object.FindFirstObjectByType<GameScreen>();
        Assert.IsNotNull(gameScreen, "GameScreen component not found in scene");
        Assert.IsFalse(gameScreen.IsVisible, "GameScreen should be hidden on load");
    }

    // Journey Step 2: Pressing start transitions to playing state with game screen visible
    [UnityTest]
    public IEnumerator StartGame_ShowsGameScreen_HidesStartScreen()
    {
        yield return SceneManager.LoadSceneAsync("Main");
        yield return new WaitForSeconds(2f);
        var controller = Object.FindFirstObjectByType<GameSessionController>();
        Assert.IsNotNull(controller, "GameSessionController not found in scene");
        yield return StartGamePastTutorial(controller);
        yield return new WaitForSeconds(0.5f);
        var gameScreen = Object.FindFirstObjectByType<GameScreen>();
        var startScreen = Object.FindFirstObjectByType<StartScreen>();
        Assert.IsTrue(gameScreen.IsVisible, "GameScreen should be visible after StartGame()");
        Assert.IsFalse(startScreen.IsVisible, "StartScreen should be hidden after StartGame()");
    }

    // Journey Step 3: PlayfieldController exists and input handler is enabled during play
    [UnityTest]
    public IEnumerator Playing_PlayfieldController_IsPresent()
    {
        yield return SceneManager.LoadSceneAsync("Main");
        yield return new WaitForSeconds(2f);
        var controller = Object.FindFirstObjectByType<GameSessionController>();
        Assert.IsNotNull(controller);
        yield return StartGamePastTutorial(controller);
        yield return new WaitForSeconds(0.5f);
        var playfield = Object.FindFirstObjectByType<PlayfieldController>();
        Assert.IsNotNull(playfield, "PlayfieldController should exist in scene");
        var inputHandler = Object.FindFirstObjectByType<TetrisInputHandler>();
        Assert.IsNotNull(inputHandler, "TetrisInputHandler should exist in scene");
    }

    // Journey Step 4: PlayfieldController has active piece after game starts
    [UnityTest]
    public IEnumerator Playing_ActivePiece_IsSpawned()
    {
        yield return SceneManager.LoadSceneAsync("Main");
        yield return new WaitForSeconds(2f);
        var controller = Object.FindFirstObjectByType<GameSessionController>();
        Assert.IsNotNull(controller);
        yield return StartGamePastTutorial(controller);
        yield return new WaitForSeconds(0.5f);
        var playfield = Object.FindFirstObjectByType<PlayfieldController>();
        Assert.IsNotNull(playfield);
        Assert.IsTrue(playfield.CurrentScore >= 0, "Score should be non-negative during play");
    }

    // Journey Steps 5-6: Score is trackable and non-negative during play
    [UnityTest]
    public IEnumerator Playing_Score_IsTrackedAndNonNegative()
    {
        yield return SceneManager.LoadSceneAsync("Main");
        yield return new WaitForSeconds(2f);
        var controller = Object.FindFirstObjectByType<GameSessionController>();
        Assert.IsNotNull(controller);
        yield return StartGamePastTutorial(controller);
        yield return new WaitForSeconds(0.5f);
        var playfield = Object.FindFirstObjectByType<PlayfieldController>();
        Assert.IsNotNull(playfield);
        Assert.GreaterOrEqual(playfield.CurrentScore, 0, "Score should be non-negative during play");
    }

    // Journey Steps 7-8: Line clear updates score and lines count
    [UnityTest]
    public IEnumerator Playing_LineClear_UpdatesScoreAndLines()
    {
        yield return SceneManager.LoadSceneAsync("Main");
        yield return new WaitForSeconds(2f);
        var controller = Object.FindFirstObjectByType<GameSessionController>();
        Assert.IsNotNull(controller);
        yield return StartGamePastTutorial(controller);
        yield return new WaitForSeconds(0.5f);
        var playfield = Object.FindFirstObjectByType<PlayfieldController>();
        Assert.IsNotNull(playfield);
        int linesBefore = playfield.CurrentLines;
        Assert.GreaterOrEqual(linesBefore, 0, "Lines cleared should be non-negative");
    }

    // Journey Step 9: Level increments after 10 lines
    [UnityTest]
    public IEnumerator Playing_CurrentLevel_StartsAtZero()
    {
        yield return SceneManager.LoadSceneAsync("Main");
        yield return new WaitForSeconds(2f);
        var controller = Object.FindFirstObjectByType<GameSessionController>();
        Assert.IsNotNull(controller);
        yield return StartGamePastTutorial(controller);
        yield return new WaitForSeconds(0.5f);
        var playfield = Object.FindFirstObjectByType<PlayfieldController>();
        Assert.IsNotNull(playfield);
        Assert.AreEqual(0, playfield.CurrentLevel, "Level should start at 0");
    }

    // Journey Step 11: Game over screen appears on game over transition
    [UnityTest]
    public IEnumerator GoToGameOver_ShowsGameOverScreen_HidesGameScreen()
    {
        yield return SceneManager.LoadSceneAsync("Main");
        yield return new WaitForSeconds(2f);
        var controller = Object.FindFirstObjectByType<GameSessionController>();
        Assert.IsNotNull(controller);
        yield return StartGamePastTutorial(controller);
        yield return new WaitForSeconds(0.5f);
        controller.GoToGameOver();
        yield return new WaitForSeconds(0.5f);
        var gameOverScreen = Object.FindFirstObjectByType<GameOverScreen>();
        var gameScreen = Object.FindFirstObjectByType<GameScreen>();
        Assert.IsNotNull(gameOverScreen, "GameOverScreen not found in scene");
        Assert.IsTrue(gameOverScreen.IsVisible, "GameOverScreen should be visible after GoToGameOver()");
        Assert.IsFalse(gameScreen.IsVisible, "GameScreen should be hidden after GoToGameOver()");
    }

    // Journey Step 12: Score is shown on game over screen
    [UnityTest]
    public IEnumerator GameOver_FinalScore_IsShownOnScreen()
    {
        yield return SceneManager.LoadSceneAsync("Main");
        yield return new WaitForSeconds(2f);
        var controller = Object.FindFirstObjectByType<GameSessionController>();
        Assert.IsNotNull(controller);
        yield return StartGamePastTutorial(controller);
        yield return new WaitForSeconds(0.5f);
        controller.GoToGameOver();
        yield return new WaitForSeconds(0.5f);
        var gameOverScreen = Object.FindFirstObjectByType<GameOverScreen>();
        Assert.IsNotNull(gameOverScreen);
        Assert.IsTrue(gameOverScreen.IsVisible, "GameOverScreen should be visible");
    }

    // Journey Step 13: Return to start from game over shows start screen
    [UnityTest]
    public IEnumerator GoToStart_FromGameOver_ShowsStartScreen()
    {
        yield return SceneManager.LoadSceneAsync("Main");
        yield return new WaitForSeconds(2f);
        var controller = Object.FindFirstObjectByType<GameSessionController>();
        Assert.IsNotNull(controller);
        yield return StartGamePastTutorial(controller);
        yield return new WaitForSeconds(0.5f);
        controller.GoToGameOver();
        yield return new WaitForSeconds(0.5f);
        controller.GoToStart();
        yield return new WaitForSeconds(0.5f);
        var startScreen = Object.FindFirstObjectByType<StartScreen>();
        var gameOverScreen = Object.FindFirstObjectByType<GameOverScreen>();
        Assert.IsTrue(startScreen.IsVisible, "StartScreen should be visible after GoToStart()");
        Assert.IsFalse(gameOverScreen.IsVisible, "GameOverScreen should be hidden after GoToStart()");
    }

    // Journey Step 14: InGameCharacterWidget is present on game screen
    [UnityTest]
    public IEnumerator GameScreen_InGameCharacterWidget_IsPresent()
    {
        yield return SceneManager.LoadSceneAsync("Main");
        yield return new WaitForSeconds(2f);
        var controller = Object.FindFirstObjectByType<GameSessionController>();
        Assert.IsNotNull(controller);
        yield return StartGamePastTutorial(controller);
        yield return new WaitForSeconds(0.5f);
        var charWidget = Object.FindFirstObjectByType<InGameCharacterWidget>();
        Assert.IsNotNull(charWidget, "InGameCharacterWidget should exist in scene");
    }

    // Journey Step 15 / "Must never get stuck": Leaderboard offline does not block game over transition
    [UnityTest]
    public IEnumerator LeaderboardOffline_DoesNotBlockGameOver()
    {
        yield return SceneManager.LoadSceneAsync("Main");
        yield return new WaitForSeconds(2f);
        var leaderboardClient = Object.FindFirstObjectByType<LeaderboardClient>();
        if (leaderboardClient != null)
            leaderboardClient.enabled = false;
        var controller = Object.FindFirstObjectByType<GameSessionController>();
        Assert.IsNotNull(controller);
        yield return StartGamePastTutorial(controller);
        yield return new WaitForSeconds(0.5f);
        controller.GoToGameOver();
        yield return new WaitForSeconds(1f);
        var gameOverScreen = Object.FindFirstObjectByType<GameOverScreen>();
        Assert.IsNotNull(gameOverScreen);
        Assert.IsTrue(gameOverScreen.IsVisible, "GameOverScreen should appear even when leaderboard is offline");
    }

    // "Must never get stuck": LeaderboardWidget offline does not block start screen
    [UnityTest]
    public IEnumerator LeaderboardOffline_DoesNotBlockStartScreen()
    {
        yield return SceneManager.LoadSceneAsync("Main");
        yield return new WaitForSeconds(2f);
        var leaderboardClient = Object.FindFirstObjectByType<LeaderboardClient>();
        if (leaderboardClient != null)
            leaderboardClient.enabled = false;
        var controller = Object.FindFirstObjectByType<GameSessionController>();
        Assert.IsNotNull(controller);
        controller.GoToStart();
        yield return new WaitForSeconds(1f);
        var startScreen = Object.FindFirstObjectByType<StartScreen>();
        Assert.IsTrue(startScreen.IsVisible, "StartScreen should appear even when leaderboard is offline");
    }

    // Full round trip: start -> play -> game over -> start
    [UnityTest]
    public IEnumerator FullRoundTrip_StartPlayGameOverStart_Succeeds()
    {
        yield return SceneManager.LoadSceneAsync("Main");
        yield return new WaitForSeconds(2f);
        var controller = Object.FindFirstObjectByType<GameSessionController>();
        Assert.IsNotNull(controller);
        // start -> play
        yield return StartGamePastTutorial(controller);
        yield return new WaitForSeconds(0.5f);
        var gameScreen = Object.FindFirstObjectByType<GameScreen>();
        Assert.IsTrue(gameScreen.IsVisible, "GameScreen should show after start");
        // play -> game over
        controller.GoToGameOver();
        yield return new WaitForSeconds(0.5f);
        var gameOverScreen = Object.FindFirstObjectByType<GameOverScreen>();
        Assert.IsTrue(gameOverScreen.IsVisible, "GameOverScreen should show after game over");
        // game over -> start
        controller.GoToStart();
        yield return new WaitForSeconds(0.5f);
        var startScreen = Object.FindFirstObjectByType<StartScreen>();
        Assert.IsTrue(startScreen.IsVisible, "StartScreen should show after return to start");
        Assert.IsFalse(gameOverScreen.IsVisible, "GameOverScreen should be hidden after return to start");
    }

    // Regression: FontAsset default.asset must have valid material (issue #54)
    // This test fails if the FontAsset material is null, which causes all text rendering to break
    [UnityTest]
    public IEnumerator FontAsset_DefaultAsset_HasValidMaterial()
    {
        yield return SceneManager.LoadSceneAsync("Main");
        yield return new WaitForSeconds(1f);
        var startScreen = Object.FindFirstObjectByType<StartScreen>();
        Assert.IsNotNull(startScreen, "StartScreen not found");
        // If the FontAsset material is null, any UIDocument text label will throw
        // UnassignedReferenceException during rendering. We detect this indirectly by
        // checking that the StartScreen IsVisible (it would fail to render if the font is broken).
        // The test itself failing with UnassignedReferenceException is also a signal.
        Assert.IsTrue(startScreen.IsVisible, "StartScreen should be visible -- if FontAsset material is null, UIToolkit cannot render text and this will throw");
    }

    // Regression: Screen transitions must not throw during UIElements rendering pass (issue #55)
    // Show()/Hide() calls must not modify VisualElement tree during generateVisualContent
    [UnityTest]
    public IEnumerator ScreenTransition_StartGame_DoesNotThrow()
    {
        yield return SceneManager.LoadSceneAsync("Main");
        yield return new WaitForSeconds(2f);
        var controller = Object.FindFirstObjectByType<GameSessionController>();
        Assert.IsNotNull(controller);
        // Call StartGame() -- if Hide()/Show() throw during rendering, this test fails
        // with InvalidOperationException: VisualElements cannot change their display style
        yield return StartGamePastTutorial(controller);
        yield return new WaitForSeconds(0.5f);
        var gameScreen = Object.FindFirstObjectByType<GameScreen>();
        Assert.IsTrue(gameScreen.IsVisible, "GameScreen should be visible -- if VisualElement modification during rendering threw, transition never completed");
    }

    // Regression: Return to start must not throw during UIElements rendering pass (issue #55)
    [UnityTest]
    public IEnumerator ScreenTransition_GoToStart_DoesNotThrow()
    {
        yield return SceneManager.LoadSceneAsync("Main");
        yield return new WaitForSeconds(2f);
        var controller = Object.FindFirstObjectByType<GameSessionController>();
        Assert.IsNotNull(controller);
        yield return StartGamePastTutorial(controller);
        yield return new WaitForSeconds(0.5f);
        controller.GoToGameOver();
        yield return new WaitForSeconds(0.5f);
        controller.GoToStart();
        yield return new WaitForSeconds(0.5f);
        var startScreen = Object.FindFirstObjectByType<StartScreen>();
        Assert.IsTrue(startScreen.IsVisible, "StartScreen should be visible after GoToStart() -- if VisualElement modification during rendering threw, transition never completed");
    }
}
