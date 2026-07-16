using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

public class StartScreenTests : InputTestFixture
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

    private StartScreen CreateScreen()
    {
        var go = new GameObject();
        go.SetActive(false);
        var uiDoc = go.AddComponent<UIDocument>();
        uiDoc.panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
        var screen = go.AddComponent<StartScreen>();
        return screen;
    }

    [UnityTest]
    public IEnumerator LeaderboardRegion_NonNull_AfterEnable()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        Assert.IsNotNull(screen.LeaderboardRegion);
        UnityEngine.Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator LeaderboardRegion_HasFiveRows()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        Assert.AreEqual(5, screen.LeaderboardRegion.childCount);
        UnityEngine.Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator LeaderboardRow_HasFourChildren()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        var row = screen.LeaderboardRegion[0];
        Assert.AreEqual(4, row.childCount);
        UnityEngine.Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator LeaderboardRow_ChildNames_AreCorrect()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        var row = screen.LeaderboardRegion[0];
        Assert.AreEqual("rank", row[0].name);
        Assert.AreEqual("score", row[1].name);
        Assert.AreEqual("username", row[2].name);
        Assert.AreEqual("characterSlot", row[3].name);
        UnityEngine.Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator LeaderboardRegion_NamedLeaderboardRegion()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        Assert.AreEqual("leaderboardRegion", screen.LeaderboardRegion.name);
        UnityEngine.Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator Show_SetsDisplayFlex()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        screen.Show();
        Assert.AreEqual(DisplayStyle.Flex, screen.Root.style.display.value);
        UnityEngine.Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator Hide_SetsDisplayNone()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        screen.Show();
        screen.Hide();
        Assert.AreEqual(DisplayStyle.None, screen.Root.style.display.value);
        UnityEngine.Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator Show_IsVisibleTrue()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        screen.Show();
        Assert.IsTrue(screen.IsVisible);
        UnityEngine.Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator Hide_IsVisibleFalse()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        screen.Show();
        screen.Hide();
        Assert.IsFalse(screen.IsVisible);
        UnityEngine.Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator Show_PromptBlinkTogglesAfterInterval()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        screen.Show();

        // title=0, prompt=1, leaderboardRegion=2
        var promptViaIndex = screen.Root[1];
        // Blink toggles style.visibility (NOT display) so the label keeps its layout
        // space and the centered column does not reflow -- assert on visibility.
        var initialVisibility = promptViaIndex.style.visibility.value;

        // Wait longer than one blink interval
        yield return new WaitForSeconds(0.9f);

        var afterVisibility = promptViaIndex.style.visibility.value;
        Assert.AreNotEqual(initialVisibility, afterVisibility,
            "Prompt visibility should have toggled after 0.8s blink interval");
        UnityEngine.Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator Hide_StopsBlink_PromptDisplayStaysFixed()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        screen.Show();

        // Wait for one blink toggle
        yield return new WaitForSeconds(0.9f);
        screen.Hide();

        var promptViaIndex = screen.Root[1];
        var displayAfterHide = promptViaIndex.style.display.value;

        // Wait another interval -- display should NOT change since coroutine was stopped
        yield return new WaitForSeconds(0.9f);
        Assert.AreEqual(displayAfterHide, promptViaIndex.style.display.value,
            "Prompt display should not change after Hide() stops the blink coroutine");
        UnityEngine.Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator OnStartPressed_FiredWhenDownArrowPressedWhileVisible()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        screen.Show();

        bool fired = false;
        screen.OnStartPressed += () => fired = true;

        Press(_keyboard.downArrowKey);
        yield return null;

        Assert.IsTrue(fired, "OnStartPressed should fire when Down arrow is pressed while visible");
        UnityEngine.Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator OnStartPressed_NotFiredWhenHidden()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        screen.Show();
        screen.Hide();

        bool fired = false;
        screen.OnStartPressed += () => fired = true;

        Press(_keyboard.downArrowKey);
        yield return null;

        Assert.IsFalse(fired, "OnStartPressed should not fire when screen is hidden");
        UnityEngine.Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator OnStartPressed_NotFiredForOtherKeys()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        screen.Show();

        bool fired = false;
        screen.OnStartPressed += () => fired = true;

        Press(_keyboard.spaceKey);
        yield return null;

        Assert.IsFalse(fired, "OnStartPressed should not fire for non-Down-arrow keys");
        UnityEngine.Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator Root_ContainsTitleLabel()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        var titleLabel = screen.Root[0] as Label;
        Assert.IsNotNull(titleLabel);
        Assert.AreEqual("TETRIS", titleLabel.text);
        UnityEngine.Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator Root_ContainsPromptLabel()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        var promptLabel = screen.Root[1] as Label;
        Assert.IsNotNull(promptLabel);
        Assert.AreEqual("PRESS DOWN OR TAP TO START", promptLabel.text);
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

    [UnityTest]
    public IEnumerator ShowHideCycle_PreservesTreeStructure()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        var initialChildCount = screen.Root.childCount;
        Assert.IsTrue(initialChildCount > 0, "Tree should be built after Start()");

        screen.Show();
        screen.Hide();
        screen.Show();
        screen.Hide();

        Assert.AreEqual(initialChildCount, screen.Root.childCount,
            "Show/Hide cycles must not rebuild the UI tree");
        UnityEngine.Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator Hide_DoesNotDeactivateGameObject()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        screen.Show();
        screen.Hide();
        Assert.IsTrue(screen.gameObject.activeSelf,
            "GameObject must remain active after Hide()");
        UnityEngine.Object.Destroy(screen.gameObject);
    }
}
