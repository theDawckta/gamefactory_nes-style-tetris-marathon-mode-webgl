using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

// HelpButtonWidget builds its '?' button inside its OWN UIDocument at sortingOrder 300
// (top of the factory layering ladder) with a pickingMode-Ignore root, so the button is
// never buried under the mobile gesture overlay (100). The widget therefore needs a
// sibling UIDocument (the GameScreen's) to source PanelSettings from, and the button is
// built one frame after Initialize().
public class HelpButtonWidgetTests
{
    private HelpButtonWidget _widget;
    private TutorialScreen _tutorialScreen;

    // Mirrors the real scene: HelpButtonWidget sits on the GameScreen GameObject, which
    // has a UIDocument with the game's PanelSettings.
    private HelpButtonWidget CreateWidget()
    {
        var go = new GameObject("HelpButtonHost");
        var doc = go.AddComponent<UIDocument>();
        doc.panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
        return go.AddComponent<HelpButtonWidget>();
    }

    private TutorialScreen CreateTutorialScreen()
    {
        var go = new GameObject();
        go.SetActive(false);
        var uiDoc = go.AddComponent<UIDocument>();
        uiDoc.panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
        var screen = go.AddComponent<TutorialScreen>();
        go.SetActive(true);
        return screen;
    }

    // Initialize starts a coroutine that yields one frame before building the button;
    // two yields guarantee it has run regardless of coroutine scheduling order.
    private static IEnumerator WaitForBuild()
    {
        yield return null;
        yield return null;
    }

    [UnityTest]
    public IEnumerator HelpButton_NonNull_AfterInitialize()
    {
        _widget = CreateWidget();
        _widget.Initialize(null, null);
        yield return WaitForBuild();
        Assert.IsNotNull(_widget.HelpButton,
            "HelpButton must be non-null after Initialize()");
        Object.Destroy(_widget.gameObject);
    }

    [UnityTest]
    public IEnumerator HelpButton_HasQuestionMarkLabel()
    {
        _widget = CreateWidget();
        _widget.Initialize(null, null);
        yield return WaitForBuild();
        Assert.AreEqual("?", _widget.HelpButton.text);
        Object.Destroy(_widget.gameObject);
    }

    [UnityTest]
    public IEnumerator HelpButton_Width_Is40()
    {
        _widget = CreateWidget();
        _widget.Initialize(null, null);
        yield return WaitForBuild();
        Assert.AreEqual(40f, _widget.HelpButton.style.width.value.value);
        Object.Destroy(_widget.gameObject);
    }

    [UnityTest]
    public IEnumerator HelpButton_Height_Is40()
    {
        _widget = CreateWidget();
        _widget.Initialize(null, null);
        yield return WaitForBuild();
        Assert.AreEqual(40f, _widget.HelpButton.style.height.value.value);
        Object.Destroy(_widget.gameObject);
    }

    [UnityTest]
    public IEnumerator HelpButton_PositionIsAbsolute()
    {
        _widget = CreateWidget();
        _widget.Initialize(null, null);
        yield return WaitForBuild();
        Assert.AreEqual(Position.Absolute, _widget.HelpButton.style.position.value);
        Object.Destroy(_widget.gameObject);
    }

    [UnityTest]
    public IEnumerator HelpButton_HasName_helpButton()
    {
        _widget = CreateWidget();
        _widget.Initialize(null, null);
        yield return WaitForBuild();
        Assert.AreEqual("helpButton", _widget.HelpButton.name);
        Object.Destroy(_widget.gameObject);
    }

    [UnityTest]
    public IEnumerator HelpButton_LivesInOwnDocument_AtSortingOrder300()
    {
        _widget = CreateWidget();
        _widget.Initialize(null, null);
        yield return WaitForBuild();

        // The layer is a ROOT GameObject (nesting a UIDocument under another UIDocument
        // forces the parent's PanelSettings -- the layer needs the overlay panel instead).
        var layer = GameObject.Find("HelpButtonLayer");
        Assert.IsNotNull(layer, "A root-level HelpButtonLayer GameObject must exist");
        var doc = layer.GetComponent<UIDocument>();
        Assert.IsNotNull(doc, "HelpButtonLayer must carry its own UIDocument");
        Assert.AreEqual(300f, doc.sortingOrder, 0.001f,
            "HelpButton document must sort above screens(0)/gesture overlay(100)/modals(200)");
        Assert.AreEqual(1f, doc.panelSettings.match, 0.001f,
            "HelpButton lives on the overlay panel (always match-height) so it stays finger-sized in portrait");
        Assert.AreSame(doc.rootVisualElement, _widget.HelpButton.parent,
            "HelpButton must be parented to its own document's root");
        Object.Destroy(_widget.gameObject);
    }

    [UnityTest]
    public IEnumerator HelpButton_DocumentRoot_IgnoresPicking()
    {
        _widget = CreateWidget();
        _widget.Initialize(null, null);
        yield return WaitForBuild();

        var doc = GameObject.Find("HelpButtonLayer").GetComponent<UIDocument>();
        Assert.AreEqual(PickingMode.Ignore, doc.rootVisualElement.pickingMode,
            "The always-on-top document root must not swallow taps meant for layers beneath");
        Assert.AreEqual(PickingMode.Position, _widget.HelpButton.pickingMode,
            "Only the button itself should pick");
        Object.Destroy(_widget.gameObject);
    }

    [UnityTest]
    public IEnumerator SetGameScreenVisible_TogglesButtonDisplay()
    {
        _widget = CreateWidget();
        _widget.Initialize(null, null);
        yield return WaitForBuild();

        Assert.AreEqual(DisplayStyle.None, _widget.HelpButton.style.display.value,
            "Button starts hidden until the GameScreen reports itself visible");
        _widget.SetGameScreenVisible(true);
        Assert.AreEqual(DisplayStyle.Flex, _widget.HelpButton.style.display.value);
        _widget.SetGameScreenVisible(false);
        Assert.AreEqual(DisplayStyle.None, _widget.HelpButton.style.display.value);
        Object.Destroy(_widget.gameObject);
    }

    [UnityTest]
    public IEnumerator NoHostDocument_DoesNotThrow_ButtonStaysNull()
    {
        // A widget on a GameObject with no UIDocument has no PanelSettings source;
        // Initialize must fail soft (no button, no exception).
        var go = new GameObject();
        _widget = go.AddComponent<HelpButtonWidget>();
        Assert.DoesNotThrow(() => _widget.Initialize(null, null));
        yield return WaitForBuild();
        Assert.IsNull(_widget.HelpButton,
            "HelpButton should stay null without a host UIDocument");
        Object.Destroy(go);
    }

    [UnityTest]
    public IEnumerator OnHide_RestoresButtonVisibility()
    {
        _widget = CreateWidget();
        _tutorialScreen = CreateTutorialScreen();
        yield return null; // wait for TutorialScreen.Start()
        _widget.Initialize(null, _tutorialScreen);
        yield return WaitForBuild();

        // Simulate what happens when the button is pressed: hide the button, show tutorial
        _widget.HelpButton.style.visibility = Visibility.Hidden;
        Assert.AreEqual(Visibility.Hidden, _widget.HelpButton.style.visibility.value,
            "Precondition: button should be hidden");

        // Simulate TutorialScreen.Hide() call (which fires OnHide)
        _tutorialScreen.Hide();

        Assert.AreEqual(Visibility.Visible, _widget.HelpButton.style.visibility.value,
            "Button must be restored to Visible after TutorialScreen.Hide() fires OnHide");

        Object.Destroy(_widget.gameObject);
        Object.Destroy(_tutorialScreen.gameObject);
    }

    [UnityTest]
    public IEnumerator OnHide_Unsubscribed_OnDestroy()
    {
        _widget = CreateWidget();
        _tutorialScreen = CreateTutorialScreen();
        yield return null;
        _widget.Initialize(null, _tutorialScreen);
        yield return WaitForBuild();

        Object.Destroy(_widget.gameObject);
        yield return null;

        // After widget is destroyed, firing OnHide should not throw
        Assert.DoesNotThrow(() => _tutorialScreen.Hide());

        Object.Destroy(_tutorialScreen.gameObject);
    }

    [UnityTest]
    public IEnumerator TutorialScreen_OnHide_FiredAfterHide()
    {
        _tutorialScreen = CreateTutorialScreen();
        yield return null;

        bool fired = false;
        _tutorialScreen.OnHide += () => { fired = true; };
        _tutorialScreen.Show();
        yield return null;
        _tutorialScreen.Hide();

        Assert.IsTrue(fired, "TutorialScreen.OnHide must fire when Hide() is called");
        Object.Destroy(_tutorialScreen.gameObject);
    }
}
