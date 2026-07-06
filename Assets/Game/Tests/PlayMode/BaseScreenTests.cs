using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

public class BaseScreenTests
{
    private class TestScreen : BaseScreen
    {
        private VisualElement _root;

        private void Awake()
        {
            _root = new VisualElement();
        }

        public override VisualElement Root => _root;
    }

    [UnityTest]
    public IEnumerator RequiresUIDocument_AutoAdded()
    {
        var go = new GameObject();
        go.AddComponent<TestScreen>();
        yield return null;
        Assert.IsNotNull(go.GetComponent<UIDocument>());
        Object.Destroy(go);
    }

    [UnityTest]
    public IEnumerator Show_SetsDisplayFlex()
    {
        var go = new GameObject();
        var screen = go.AddComponent<TestScreen>();
        yield return null;
        screen.Show();
        Assert.AreEqual(DisplayStyle.Flex, screen.Root.style.display.value);
        Object.Destroy(go);
    }

    [UnityTest]
    public IEnumerator Hide_SetsDisplayNone()
    {
        var go = new GameObject();
        var screen = go.AddComponent<TestScreen>();
        yield return null;
        screen.Show();
        screen.Hide();
        Assert.AreEqual(DisplayStyle.None, screen.Root.style.display.value);
        Object.Destroy(go);
    }

    [UnityTest]
    public IEnumerator IsVisible_TrueAfterShow()
    {
        var go = new GameObject();
        var screen = go.AddComponent<TestScreen>();
        yield return null;
        screen.Show();
        Assert.IsTrue(screen.IsVisible);
        Object.Destroy(go);
    }

    [UnityTest]
    public IEnumerator IsVisible_FalseAfterHide()
    {
        var go = new GameObject();
        var screen = go.AddComponent<TestScreen>();
        yield return null;
        screen.Show();
        screen.Hide();
        Assert.IsFalse(screen.IsVisible);
        Object.Destroy(go);
    }

    [UnityTest]
    public IEnumerator ShowTwice_StaysVisible()
    {
        var go = new GameObject();
        var screen = go.AddComponent<TestScreen>();
        yield return null;
        screen.Show();
        screen.Show();
        Assert.IsTrue(screen.IsVisible);
        Object.Destroy(go);
    }

    [UnityTest]
    public IEnumerator HideTwice_StaysHidden()
    {
        var go = new GameObject();
        var screen = go.AddComponent<TestScreen>();
        yield return null;
        screen.Show();
        screen.Hide();
        screen.Hide();
        Assert.IsFalse(screen.IsVisible);
        Object.Destroy(go);
    }
}
