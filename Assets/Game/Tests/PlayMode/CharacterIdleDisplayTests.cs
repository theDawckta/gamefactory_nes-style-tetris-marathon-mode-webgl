using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

public class CharacterIdleDisplayTests
{
    private GameObject _go;

    [TearDown]
    public void TearDown()
    {
        if (_go != null) Object.Destroy(_go);
    }

    [UnityTest]
    public IEnumerator CharacterIdleDisplay_AttachesToGameObject()
    {
        _go = new GameObject();
        var display = _go.AddComponent<CharacterIdleDisplay>();
        yield return null;
        Assert.IsNotNull(display);
    }

    [UnityTest]
    public IEnumerator RootProperty_CanBeAssignedAndRead()
    {
        _go = new GameObject();
        var display = _go.AddComponent<CharacterIdleDisplay>();
        yield return null;

        var root = new VisualElement();
        display.Root = root;

        Assert.AreSame(root, display.Root);
    }

    [UnityTest]
    public IEnumerator Hide_SetsDisplayToNone_WhenRootIsSet()
    {
        _go = new GameObject();
        var display = _go.AddComponent<CharacterIdleDisplay>();
        yield return null;

        var root = new VisualElement();
        root.style.display = DisplayStyle.Flex;
        display.Root = root;

        display.Hide();

        Assert.AreEqual(DisplayStyle.None, root.style.display.value);
    }

    [UnityTest]
    public IEnumerator Show_SetsDisplayToFlex_WhenRootIsSet()
    {
        _go = new GameObject();
        var display = _go.AddComponent<CharacterIdleDisplay>();
        yield return null;

        var root = new VisualElement();
        root.style.display = DisplayStyle.None;
        display.Root = root;

        display.Show();

        Assert.AreEqual(DisplayStyle.Flex, root.style.display.value);
    }

    [UnityTest]
    public IEnumerator Hide_DoesNotThrow_WhenRootIsNull()
    {
        _go = new GameObject();
        var display = _go.AddComponent<CharacterIdleDisplay>();
        yield return null;
        display.Root = null;

        Assert.DoesNotThrow(() => display.Hide());
        yield return null;
    }

    [UnityTest]
    public IEnumerator Show_DoesNotThrow_WhenRootIsNull()
    {
        _go = new GameObject();
        var display = _go.AddComponent<CharacterIdleDisplay>();
        yield return null;
        display.Root = null;

        Assert.DoesNotThrow(() => display.Show());
        yield return null;
    }

    [UnityTest]
    public IEnumerator Update_DoesNotThrow_WhenRootIsNull()
    {
        _go = new GameObject();
        _go.AddComponent<CharacterIdleDisplay>();
        // Root is null by default -- Update() must not throw.
        yield return null;
        yield return null;
    }

    [UnityTest]
    public IEnumerator Update_DoesNotModifyBackground_WhenFramesNotLoaded()
    {
        _go = new GameObject();
        var display = _go.AddComponent<CharacterIdleDisplay>();
        yield return null;

        var root = new VisualElement();
        display.Root = root;
        display.Show();

        var initialKeyword = root.style.backgroundImage.keyword;

        // Advance several frames -- no frames loaded, so background should remain unchanged.
        yield return null;
        yield return null;

        Assert.AreEqual(initialKeyword, root.style.backgroundImage.keyword);
    }

    [UnityTest]
    public IEnumerator Update_DoesNotModifyBackground_WhenHidden()
    {
        _go = new GameObject();
        var display = _go.AddComponent<CharacterIdleDisplay>();
        yield return null;

        var root = new VisualElement();
        display.Root = root;
        display.Hide();

        var initialKeyword = root.style.backgroundImage.keyword;

        yield return null;
        yield return null;

        Assert.AreEqual(initialKeyword, root.style.backgroundImage.keyword);
    }

    [UnityTest]
    public IEnumerator Load_DoesNotThrow_WhenRootIsNull()
    {
        _go = new GameObject();
        var display = _go.AddComponent<CharacterIdleDisplay>();
        yield return null;
        display.Root = null;

        Assert.DoesNotThrow(() => display.Load("testchar", "http://localhost:29999"));
        // Allow the coroutine to start and fail gracefully.
        yield return null;
    }

    [UnityTest]
    public IEnumerator Load_CallsHide_WhenAnimationsRequestFails()
    {
        _go = new GameObject();
        var display = _go.AddComponent<CharacterIdleDisplay>();
        yield return null;

        var root = new VisualElement();
        root.style.display = DisplayStyle.Flex;
        display.Root = root;
        display.Show();

        // Port 29999 on localhost is not in use -- connection refused is near-instant.
        display.Load("testchar", "http://localhost:29999");

        // Wait until Hidden or timeout (10 seconds is well above the 5s request timeout).
        float elapsed = 0f;
        while (root.style.display.value != DisplayStyle.None && elapsed < 10f)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        Assert.AreEqual(DisplayStyle.None, root.style.display.value);
    }

    [UnityTest]
    public IEnumerator ShowThenHide_TogglesDisplayCorrectly()
    {
        _go = new GameObject();
        var display = _go.AddComponent<CharacterIdleDisplay>();
        yield return null;

        var root = new VisualElement();
        display.Root = root;

        display.Show();
        Assert.AreEqual(DisplayStyle.Flex, root.style.display.value);

        display.Hide();
        Assert.AreEqual(DisplayStyle.None, root.style.display.value);

        display.Show();
        Assert.AreEqual(DisplayStyle.Flex, root.style.display.value);
    }
}
