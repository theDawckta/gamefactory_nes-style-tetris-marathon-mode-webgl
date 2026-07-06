using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

public class CharacterIdleDisplayTests
{
    [UnityTest]
    public IEnumerator CharacterIdleDisplay_AttachesToGameObject()
    {
        var go = new GameObject();
        var display = go.AddComponent<CharacterIdleDisplay>();
        yield return null;
        Assert.IsNotNull(display);
        Object.Destroy(go);
    }

    [UnityTest]
    public IEnumerator Root_PropertyAssignment_RoundTrips()
    {
        var go = new GameObject();
        var display = go.AddComponent<CharacterIdleDisplay>();
        var root = new VisualElement();
        display.Root = root;
        yield return null;
        Assert.AreSame(root, display.Root);
        Object.Destroy(go);
    }

    [UnityTest]
    public IEnumerator Hide_SetsDisplayStyleNone()
    {
        var go = new GameObject();
        var display = go.AddComponent<CharacterIdleDisplay>();
        var root = new VisualElement();
        root.style.display = DisplayStyle.Flex;
        display.Root = root;
        yield return null;
        display.Hide();
        Assert.AreEqual(DisplayStyle.None, root.style.display.value);
        Object.Destroy(go);
    }

    [UnityTest]
    public IEnumerator Show_SetsDisplayStyleFlex()
    {
        var go = new GameObject();
        var display = go.AddComponent<CharacterIdleDisplay>();
        var root = new VisualElement();
        root.style.display = DisplayStyle.None;
        display.Root = root;
        yield return null;
        display.Show();
        Assert.AreEqual(DisplayStyle.Flex, root.style.display.value);
        Object.Destroy(go);
    }

    [UnityTest]
    public IEnumerator Hide_WithNullRoot_DoesNotThrow()
    {
        var go = new GameObject();
        var display = go.AddComponent<CharacterIdleDisplay>();
        yield return null;
        Assert.DoesNotThrow(() => display.Hide());
        Object.Destroy(go);
    }

    [UnityTest]
    public IEnumerator Show_WithNullRoot_DoesNotThrow()
    {
        var go = new GameObject();
        var display = go.AddComponent<CharacterIdleDisplay>();
        yield return null;
        Assert.DoesNotThrow(() => display.Show());
        Object.Destroy(go);
    }

    [UnityTest]
    public IEnumerator Load_WithNullRoot_DoesNotThrow()
    {
        var go = new GameObject();
        var display = go.AddComponent<CharacterIdleDisplay>();
        yield return null;
        Assert.DoesNotThrow(() => display.Load("test", "http://localhost:9"));
        // Allow any pending coroutine to settle without blocking the test
        yield return null;
        Object.Destroy(go);
    }

    [UnityTest]
    public IEnumerator Update_WithNoFramesLoaded_DoesNotSetBackground()
    {
        var go = new GameObject();
        var display = go.AddComponent<CharacterIdleDisplay>();
        var root = new VisualElement();
        display.Root = root;
        display.Show();
        yield return null;
        // backgroundImage should remain at its default (never set) keyword
        Assert.AreEqual(StyleKeyword.Null, root.style.backgroundImage.keyword);
        Object.Destroy(go);
    }

    [UnityTest]
    public IEnumerator Update_WhenHidden_DoesNotSetBackground()
    {
        var go = new GameObject();
        var display = go.AddComponent<CharacterIdleDisplay>();
        var root = new VisualElement();
        display.Root = root;

        // Inject frames via reflection so Update would normally run
        var tex = new Texture2D(32, 32);
        var sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0f));
        typeof(CharacterIdleDisplay).GetField("_frames", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(display, new Sprite[] { sprite });
        typeof(CharacterIdleDisplay).GetField("_frameCount", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(display, 1);
        typeof(CharacterIdleDisplay).GetField("_fps", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(display, 8f);

        // Explicitly hide so Update() skips animation
        display.Hide();
        yield return null;
        Assert.AreEqual(StyleKeyword.Null, root.style.backgroundImage.keyword);
        Object.Destroy(go);
    }

    [UnityTest]
    public IEnumerator Load_HidesWhenAnimationsRequestFails()
    {
        var go = new GameObject();
        var display = go.AddComponent<CharacterIdleDisplay>();
        var root = new VisualElement();
        root.style.display = DisplayStyle.Flex;
        display.Root = root;
        yield return null;

        display.Load("test", "http://localhost:9");
        // Wait for the network request to fail (connection refused is near-instant)
        float elapsed = 0f;
        while (root.style.display.value != DisplayStyle.None && elapsed < 5f)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        Assert.AreEqual(DisplayStyle.None, root.style.display.value);
        Object.Destroy(go);
    }

    [UnityTest]
    public IEnumerator ShowHide_TogglesDisplayStyle()
    {
        var go = new GameObject();
        var display = go.AddComponent<CharacterIdleDisplay>();
        var root = new VisualElement();
        display.Root = root;
        yield return null;

        display.Show();
        Assert.AreEqual(DisplayStyle.Flex, root.style.display.value);

        display.Hide();
        Assert.AreEqual(DisplayStyle.None, root.style.display.value);

        display.Show();
        Assert.AreEqual(DisplayStyle.Flex, root.style.display.value);
        Object.Destroy(go);
    }

    [UnityTest]
    public IEnumerator Hide_CalledTwice_NoException()
    {
        var go = new GameObject();
        var display = go.AddComponent<CharacterIdleDisplay>();
        var root = new VisualElement();
        display.Root = root;
        yield return null;
        Assert.DoesNotThrow(() => { display.Hide(); display.Hide(); });
        Assert.AreEqual(DisplayStyle.None, root.style.display.value);
        Object.Destroy(go);
    }
}
