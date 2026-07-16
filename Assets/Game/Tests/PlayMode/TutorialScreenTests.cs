using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

public class TutorialScreenTests
{
    private TutorialScreen CreateScreen()
    {
        var go = new GameObject();
        go.SetActive(false);
        var uiDoc = go.AddComponent<UIDocument>();
        uiDoc.panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
        var screen = go.AddComponent<TutorialScreen>();
        return screen;
    }

    [UnityTest]
    public IEnumerator RequiresUIDocument_ComponentPresent()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        Assert.IsNotNull(screen.GetComponent<UIDocument>());
        Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator Container_ExistsAfterEnable()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        Assert.IsNotNull(screen.Root.Q<VisualElement>("container"),
            "container VisualElement must exist after Start()");
        Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator TapTarget_ExistsAfterEnable()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        Assert.IsNotNull(screen.Root.Q<VisualElement>("tapTarget"),
            "tapTarget VisualElement must exist after Start()");
        Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator CloseButton_ExistsInContainer()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        var container = screen.Root.Q<VisualElement>("container");
        Assert.IsNotNull(container.Q<Button>("closeButton"),
            "closeButton must be a child of container");
        Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator CloseButton_HasXLabel()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        var btn = screen.Root.Q<Button>("closeButton");
        Assert.AreEqual("X", btn.text);
        Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator Diagram_ExistsInContainer()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        var container = screen.Root.Q<VisualElement>("container");
        Assert.IsNotNull(container.Q<VisualElement>("diagram"),
            "diagram VisualElement must be a child of container");
        Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator Diagram_HasExpectedDimensions()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        var diagram = screen.Root.Q<VisualElement>("diagram");
        // Fills the container's width; fixed 200-unit height on the overlay panel.
        Assert.AreEqual(LengthUnit.Percent, diagram.style.width.value.unit);
        Assert.AreEqual(100f, diagram.style.width.value.value);
        Assert.AreEqual(200f, diagram.style.height.value.value);
        Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator Diagram_WithoutTexture_BuildsProceduralContent()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        var diagram = screen.Root.Q<VisualElement>("diagram");
        Assert.IsNotNull(diagram.Q<VisualElement>("diagramRow"),
            "With no texture assigned, the procedural gesture diagram must be built");
        Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator UIDocument_UsesOverlayPanel_AtSortingOrder200()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        var doc = screen.GetComponent<UIDocument>();
        Assert.AreEqual(200f, doc.sortingOrder, 0.001f,
            "Tutorial must sort above screens(0) and the gesture overlay(100)");
        Assert.AreEqual(1f, doc.panelSettings.match, 0.001f,
            "Tutorial lives on the overlay panel (always match-height) so it never shrinks with the portrait width-fit");
        Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator DismissLabel_ExistsInContainer()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        var container = screen.Root.Q<VisualElement>("container");
        Assert.IsNotNull(container.Q<Label>("dismissLabel"),
            "dismissLabel must be a child of container");
        Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator Show_SetsDisplayFlex()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        screen.Show();
        Assert.AreEqual(DisplayStyle.Flex, screen.Root.style.display.value);
        Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator Show_IsVisibleTrue()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        screen.Show();
        Assert.IsTrue(screen.IsVisible);
        Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator Hide_SetsDisplayNone()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        screen.Show();
        yield return null; // advance past shown frame
        screen.Hide();
        Assert.AreEqual(DisplayStyle.None, screen.Root.style.display.value);
        Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator Hide_IsVisibleFalse()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        screen.Show();
        yield return null;
        screen.Hide();
        Assert.IsFalse(screen.IsVisible);
        Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator Hide_DoesNotDeactivateGameObject()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        screen.Show();
        yield return null;
        screen.Hide();
        Assert.IsTrue(screen.gameObject.activeSelf,
            "Hide() must not deactivate the GameObject");
        Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator TutorialSeen_DefaultIsZero()
    {
        PlayerPrefs.DeleteKey("tetris_tutorial_seen");
        Assert.AreEqual(0, PlayerPrefs.GetInt("tetris_tutorial_seen", 0));
        yield return null;
    }

    [UnityTest]
    public IEnumerator Dismiss_SetsPlayerPrefsFlag()
    {
        PlayerPrefs.DeleteKey("tetris_tutorial_seen");
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        screen.Show();
        yield return null; // advance past the shown frame
        screen.Dismiss();
        Assert.AreEqual(1, PlayerPrefs.GetInt("tetris_tutorial_seen", 0));
        Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator Dismiss_HidesScreen()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        screen.Show();
        yield return null;
        screen.Dismiss();
        Assert.IsFalse(screen.IsVisible, "Dismiss() must hide the screen");
        Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator Dismiss_OnShownFrame_DoesNotHide()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        screen.Show();
        // Dismiss() on the same frame as Show() -- cascade guard must block it.
        screen.Dismiss();
        Assert.IsTrue(screen.IsVisible,
            "Dismiss() on the same frame as Show() must be blocked by the cascade guard");
        Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator Dismiss_OnLaterFrame_Succeeds()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        screen.Show();
        yield return null; // advance to next frame
        screen.Dismiss();
        Assert.IsFalse(screen.IsVisible,
            "Dismiss() called after the shown frame must hide the screen");
        Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator ShowHideCycle_PreservesTreeStructure()
    {
        var screen = CreateScreen();
        screen.gameObject.SetActive(true);
        yield return null;
        var initialChildCount = screen.Root.childCount;
        Assert.IsTrue(initialChildCount > 0, "UI tree must be built after Start()");
        screen.Show();
        yield return null;
        screen.Hide();
        screen.Show();
        yield return null;
        screen.Hide();
        Assert.AreEqual(initialChildCount, screen.Root.childCount,
            "Show/Hide cycles must not rebuild the UI tree");
        Object.Destroy(screen.gameObject);
    }

    [UnityTest]
    public IEnumerator PlayfieldController_Pause_StopsIsRunning()
    {
        var pcGo = new GameObject();
        var pc = pcGo.AddComponent<PlayfieldController>();
        yield return null;
        pc.StartGame();
        Assert.IsTrue(pc.IsRunning);
        pc.Pause();
        Assert.IsFalse(pc.IsRunning, "Pause() must set IsRunning to false");
        Object.Destroy(pcGo);
    }

    [UnityTest]
    public IEnumerator PlayfieldController_Resume_RestoresIsRunning()
    {
        var pcGo = new GameObject();
        var pc = pcGo.AddComponent<PlayfieldController>();
        yield return null;
        pc.StartGame();
        pc.Pause();
        Assert.IsFalse(pc.IsRunning);
        pc.Resume();
        Assert.IsTrue(pc.IsRunning, "Resume() must set IsRunning to true");
        Object.Destroy(pcGo);
    }
}
