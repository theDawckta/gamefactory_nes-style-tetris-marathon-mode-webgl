using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

public class GameOverScreenTests : InputTestFixture
{
    private Keyboard _keyboard;

    [SetUp]
    public override void Setup()
    {
        base.Setup();
        _keyboard = InputSystem.AddDevice<Keyboard>();
    }

    [TearDown]
    public override void TearDown()
    {
        base.TearDown();
    }

    private GameOverScreen CreateScreen()
    {
        var go = new GameObject();
        go.SetActive(false);
        var uiDoc = go.AddComponent<UIDocument>();
        uiDoc.panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
        var screen = go.AddComponent<GameOverScreen>();
        return screen;
    }

    [UnityTest]
    public IEnumerator FinalScoreRegion_NonNull_AfterEnable()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        Assert.IsNotNull(screen.Root.Q<VisualElement>("finalScoreRegion"));
        UnityEngine.Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator HighScoreBannerRegion_NonNull_AfterEnable()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        Assert.IsNotNull(screen.Root.Q<VisualElement>("highScoreBannerRegion"));
        UnityEngine.Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator HighScoreBannerRegion_HiddenByDefault()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        var banner = screen.Root.Q<VisualElement>("highScoreBannerRegion");
        Assert.AreEqual(DisplayStyle.None, banner.style.display.value);
        UnityEngine.Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator ShowWithResult_SetsScoreValue()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        screen.ShowWithResult(12345, false);
        var scoreRegion = screen.Root.Q<VisualElement>("finalScoreRegion");
        var valueLabel = scoreRegion.Q<Label>("scoreValue");
        Assert.AreEqual("12345", valueLabel.text);
        UnityEngine.Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator ShowWithResult_ShowsHighScoreBanner_WhenNewHighScore()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        screen.ShowWithResult(99999, true);
        var banner = screen.Root.Q<VisualElement>("highScoreBannerRegion");
        Assert.AreEqual(DisplayStyle.Flex, banner.style.display.value);
        UnityEngine.Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator ShowWithResult_HidesHighScoreBanner_WhenNotNewHighScore()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        screen.ShowWithResult(99999, true);
        screen.ShowWithResult(100, false);
        var banner = screen.Root.Q<VisualElement>("highScoreBannerRegion");
        Assert.AreEqual(DisplayStyle.None, banner.style.display.value);
        UnityEngine.Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator ShowWithResult_SetsDisplayFlex()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        screen.ShowWithResult(0, false);
        Assert.AreEqual(DisplayStyle.Flex, screen.Root.style.display.value);
        UnityEngine.Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator ShowWithResult_IsVisibleTrue()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        screen.ShowWithResult(0, false);
        Assert.IsTrue(screen.IsVisible);
        UnityEngine.Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator Hide_SetsDisplayNone()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        screen.ShowWithResult(0, false);
        screen.Hide();
        Assert.AreEqual(DisplayStyle.None, screen.Root.style.display.value);
        UnityEngine.Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator Hide_IsVisibleFalse()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        screen.ShowWithResult(0, false);
        screen.Hide();
        Assert.IsFalse(screen.IsVisible);
        UnityEngine.Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator OnReturnPressed_FiredWhenDownArrowPressedWhileVisible()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        screen.ShowWithResult(0, false);

        bool fired = false;
        screen.OnReturnPressed += () => fired = true;

        Press(_keyboard.downArrowKey);
        yield return null;

        Assert.IsTrue(fired, "OnReturnPressed should fire when Down arrow is pressed while visible");
        UnityEngine.Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator OnReturnPressed_NotFiredWhenHidden()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        screen.ShowWithResult(0, false);
        screen.Hide();

        bool fired = false;
        screen.OnReturnPressed += () => fired = true;

        Press(_keyboard.downArrowKey);
        yield return null;

        Assert.IsFalse(fired, "OnReturnPressed should not fire when hidden");
        UnityEngine.Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator OnReturnPressed_NotFiredForOtherKeys()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        screen.ShowWithResult(0, false);

        bool fired = false;
        screen.OnReturnPressed += () => fired = true;

        Press(_keyboard.spaceKey);
        yield return null;

        Assert.IsFalse(fired, "OnReturnPressed should not fire for non-Down-arrow keys");
        UnityEngine.Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator ShowWithResult_PromptBlinkTogglesAfterInterval()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        screen.ShowWithResult(0, false);

        // root children: gameOverLabel=0, finalScoreRegion=1, highScoreBannerRegion=2, promptLabel=3
        var promptLabel = screen.Root[3];
        var initialDisplay = promptLabel.style.display.value;

        yield return new WaitForSeconds(0.9f);

        var afterDisplay = promptLabel.style.display.value;
        Assert.AreNotEqual(initialDisplay, afterDisplay,
            "Prompt display should have toggled after 0.8s blink interval");
        UnityEngine.Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator Hide_StopsBlink_PromptDisplayStaysFixed()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        screen.ShowWithResult(0, false);

        yield return new WaitForSeconds(0.9f);
        screen.Hide();

        // root children: gameOverLabel=0, finalScoreRegion=1, highScoreBannerRegion=2, promptLabel=3
        var promptLabel = screen.Root[3];
        var displayAfterHide = promptLabel.style.display.value;

        yield return new WaitForSeconds(0.9f);
        Assert.AreEqual(displayAfterHide, promptLabel.style.display.value,
            "Prompt display should not change after Hide() stops the blink coroutine");
        UnityEngine.Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator Root_ContainsGameOverLabel()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        var label = screen.Root[0] as Label;
        Assert.IsNotNull(label);
        Assert.AreEqual("GAME OVER", label.text);
        UnityEngine.Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator RequiresUIDocument_ComponentPresent()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        Assert.IsNotNull(screen.GetComponent<UIDocument>());
        UnityEngine.Object.Destroy(screen.gameObject);
    }
}
