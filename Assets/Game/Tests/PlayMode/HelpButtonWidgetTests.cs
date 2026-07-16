using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

public class HelpButtonWidgetTests
{
    private HelpButtonWidget _widget;
    private TutorialScreen _tutorialScreen;
    private VisualElement _root;

    private HelpButtonWidget CreateWidget()
    {
        var go = new GameObject();
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

    [UnityTest]
    public IEnumerator HelpButton_NonNull_AfterInitialize()
    {
        _widget = CreateWidget();
        _root = new VisualElement();
        _widget.Initialize(_root, null);
        yield return null;
        Assert.IsNotNull(_widget.HelpButton,
            "HelpButton must be non-null after Initialize()");
        Object.Destroy(_widget.gameObject);
    }

    [UnityTest]
    public IEnumerator HelpButton_HasQuestionMarkLabel()
    {
        _widget = CreateWidget();
        _root = new VisualElement();
        _widget.Initialize(_root, null);
        yield return null;
        Assert.AreEqual("?", _widget.HelpButton.text);
        Object.Destroy(_widget.gameObject);
    }

    [UnityTest]
    public IEnumerator HelpButton_Width_Is40()
    {
        _widget = CreateWidget();
        _root = new VisualElement();
        _widget.Initialize(_root, null);
        yield return null;
        Assert.AreEqual(40f, _widget.HelpButton.style.width.value.value);
        Object.Destroy(_widget.gameObject);
    }

    [UnityTest]
    public IEnumerator HelpButton_Height_Is40()
    {
        _widget = CreateWidget();
        _root = new VisualElement();
        _widget.Initialize(_root, null);
        yield return null;
        Assert.AreEqual(40f, _widget.HelpButton.style.height.value.value);
        Object.Destroy(_widget.gameObject);
    }

    [UnityTest]
    public IEnumerator HelpButton_PositionIsAbsolute()
    {
        _widget = CreateWidget();
        _root = new VisualElement();
        _widget.Initialize(_root, null);
        yield return null;
        Assert.AreEqual(Position.Absolute, _widget.HelpButton.style.position.value);
        Object.Destroy(_widget.gameObject);
    }

    [UnityTest]
    public IEnumerator HelpButton_HasName_helpButton()
    {
        _widget = CreateWidget();
        _root = new VisualElement();
        _widget.Initialize(_root, null);
        yield return null;
        Assert.AreEqual("helpButton", _widget.HelpButton.name);
        Object.Destroy(_widget.gameObject);
    }

    [UnityTest]
    public IEnumerator HelpButton_AddedToRoot()
    {
        _widget = CreateWidget();
        _root = new VisualElement();
        _widget.Initialize(_root, null);
        yield return null;
        Assert.AreEqual(1, _root.childCount,
            "Root must have exactly one child after Initialize()");
        Assert.AreSame(_widget.HelpButton, _root[0]);
        Object.Destroy(_widget.gameObject);
    }

    [UnityTest]
    public IEnumerator HelpButton_VisibleByDefault()
    {
        _widget = CreateWidget();
        _root = new VisualElement();
        _widget.Initialize(_root, null);
        yield return null;
        Assert.AreNotEqual(Visibility.Hidden, _widget.HelpButton.style.visibility.value,
            "HelpButton must be visible by default");
        Object.Destroy(_widget.gameObject);
    }

    [UnityTest]
    public IEnumerator NullRoot_DoesNotThrow()
    {
        _widget = CreateWidget();
        Assert.DoesNotThrow(() => _widget.Initialize(null, null));
        yield return null;
        Assert.IsNull(_widget.HelpButton,
            "HelpButton should be null when Initialize is given a null root");
        Object.Destroy(_widget.gameObject);
    }

    [UnityTest]
    public IEnumerator OnHide_RestoresButtonVisibility()
    {
        _widget = CreateWidget();
        _root = new VisualElement();
        _tutorialScreen = CreateTutorialScreen();
        yield return null; // wait for TutorialScreen.Start()
        _widget.Initialize(_root, _tutorialScreen);
        yield return null;

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
        _root = new VisualElement();
        _tutorialScreen = CreateTutorialScreen();
        yield return null;
        _widget.Initialize(_root, _tutorialScreen);
        yield return null;

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
