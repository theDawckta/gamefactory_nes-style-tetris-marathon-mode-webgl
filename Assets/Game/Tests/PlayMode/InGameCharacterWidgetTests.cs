using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

public class InGameCharacterWidgetTests
{
    private static readonly BindingFlags NonPubInst = BindingFlags.NonPublic | BindingFlags.Instance;

    // Creates a widget with fields set before Awake runs, so subscriptions wire correctly.
    private static (GameObject go, InGameCharacterWidget widget) CreateWidget(
        FactoryAuthController authController,
        CharacterIdleDisplay charIdleDisplay)
    {
        var go = new GameObject("InGameCharacterWidget");
        go.SetActive(false);
        var widget = go.AddComponent<InGameCharacterWidget>();
        if (authController != null)
            typeof(InGameCharacterWidget).GetField("_authController", NonPubInst)?.SetValue(widget, authController);
        if (charIdleDisplay != null)
            typeof(InGameCharacterWidget).GetField("_charIdleDisplay", NonPubInst)?.SetValue(widget, charIdleDisplay);
        go.SetActive(true); // Awake runs now with fields populated
        return (go, widget);
    }

    [UnityTest]
    public IEnumerator AttachesToGameObject()
    {
        var go = new GameObject();
        var widget = go.AddComponent<InGameCharacterWidget>();
        yield return null;
        Assert.IsNotNull(widget);
        UnityEngine.Object.Destroy(go);
    }

    [UnityTest]
    public IEnumerator Initialize_SetsCharIdleDisplayRoot()
    {
        var displayGo = new GameObject();
        var charDisplay = displayGo.AddComponent<CharacterIdleDisplay>();

        var (widgetGo, widget) = CreateWidget(null, charDisplay);
        yield return null;

        var root = new VisualElement();
        widget.Initialize(root);

        Assert.AreSame(root, charDisplay.Root);

        UnityEngine.Object.Destroy(widgetGo);
        UnityEngine.Object.Destroy(displayGo);
    }

    [UnityTest]
    public IEnumerator Initialize_KeepsRegionHidden()
    {
        var displayGo = new GameObject();
        var charDisplay = displayGo.AddComponent<CharacterIdleDisplay>();

        var (widgetGo, widget) = CreateWidget(null, charDisplay);
        yield return null;

        var root = new VisualElement();
        root.style.display = DisplayStyle.Flex;
        widget.Initialize(root);

        Assert.AreEqual(DisplayStyle.None, root.style.display.value);

        UnityEngine.Object.Destroy(widgetGo);
        UnityEngine.Object.Destroy(displayGo);
    }

    [UnityTest]
    public IEnumerator Initialize_WithNullAuthController_DoesNotThrow()
    {
        var displayGo = new GameObject();
        var charDisplay = displayGo.AddComponent<CharacterIdleDisplay>();

        var (widgetGo, widget) = CreateWidget(null, charDisplay);
        yield return null;

        var root = new VisualElement();
        Assert.DoesNotThrow(() => widget.Initialize(root));

        UnityEngine.Object.Destroy(widgetGo);
        UnityEngine.Object.Destroy(displayGo);
    }

    [UnityTest]
    public IEnumerator GuestResolution_NullCharacterName_LeavesRegionHidden()
    {
        var authGo = new GameObject();
        var controller = authGo.AddComponent<FactoryAuthController>();

        var displayGo = new GameObject();
        var charDisplay = displayGo.AddComponent<CharacterIdleDisplay>();

        var (widgetGo, widget) = CreateWidget(controller, charDisplay);

        var root = new VisualElement();
        widget.Initialize(root);

        yield return null; // FactoryAuthController resolves as guest (CharacterName=null); handler is no-op

        Assert.AreEqual(DisplayStyle.None, root.style.display.value,
            "Region must remain hidden when CharacterName is null");

        UnityEngine.Object.Destroy(widgetGo);
        UnityEngine.Object.Destroy(displayGo);
        UnityEngine.Object.Destroy(authGo);
    }

    [UnityTest]
    public IEnumerator NullCharacterName_HandlerIsNoOp()
    {
        var authGo = new GameObject();
        var controller = authGo.AddComponent<FactoryAuthController>();

        var displayGo = new GameObject();
        var charDisplay = displayGo.AddComponent<CharacterIdleDisplay>();

        var (widgetGo, widget) = CreateWidget(controller, charDisplay);

        var root = new VisualElement();
        widget.Initialize(root);

        // Force region visible so we can detect if the handler incorrectly hides or changes it
        root.style.display = DisplayStyle.Flex;

        // CharacterName is null by default; fire event to exercise the early-return path
        var del = typeof(FactoryAuthController)
            .GetField("OnIdentityResolved", NonPubInst)
            ?.GetValue(controller) as System.Action<FactoryAuthController>;
        del?.Invoke(controller);

        yield return null;

        // Handler must be a no-op: region stays Flex
        Assert.AreEqual(DisplayStyle.Flex, root.style.display.value,
            "Null CharacterName must leave region state unchanged");

        UnityEngine.Object.Destroy(widgetGo);
        UnityEngine.Object.Destroy(displayGo);
        UnityEngine.Object.Destroy(authGo);
    }

    [UnityTest]
    public IEnumerator EmptyCharacterName_HandlerIsNoOp()
    {
        var authGo = new GameObject();
        var controller = authGo.AddComponent<FactoryAuthController>();

        var displayGo = new GameObject();
        var charDisplay = displayGo.AddComponent<CharacterIdleDisplay>();

        var (widgetGo, widget) = CreateWidget(controller, charDisplay);

        var root = new VisualElement();
        widget.Initialize(root);
        root.style.display = DisplayStyle.Flex;

        // Set CharacterName to "" via the backing field
        typeof(FactoryAuthController)
            .GetField("<CharacterName>k__BackingField", NonPubInst)
            ?.SetValue(controller, "");

        var del = typeof(FactoryAuthController)
            .GetField("OnIdentityResolved", NonPubInst)
            ?.GetValue(controller) as System.Action<FactoryAuthController>;
        del?.Invoke(controller);

        yield return null;

        Assert.AreEqual(DisplayStyle.Flex, root.style.display.value,
            "Empty CharacterName must leave region state unchanged");

        UnityEngine.Object.Destroy(widgetGo);
        UnityEngine.Object.Destroy(displayGo);
        UnityEngine.Object.Destroy(authGo);
    }

    [UnityTest]
    public IEnumerator NonNullCharacterName_CallsLoad_HidesOnNetworkFailure()
    {
        var authGo = new GameObject();
        var controller = authGo.AddComponent<FactoryAuthController>();

        var displayGo = new GameObject();
        var charDisplay = displayGo.AddComponent<CharacterIdleDisplay>();

        var (widgetGo, widget) = CreateWidget(controller, charDisplay);

        var root = new VisualElement();
        widget.Initialize(root);

        yield return null; // initial guest resolution: null CharacterName, no-op

        // Set CharacterName to a non-null value via backing field
        typeof(FactoryAuthController)
            .GetField("<CharacterName>k__BackingField", NonPubInst)
            ?.SetValue(controller, "testchar");

        // Force region visible so we can detect when Load() fails and calls Hide()
        root.style.display = DisplayStyle.Flex;

        // Fire OnIdentityResolved so the widget calls Load()
        var del = typeof(FactoryAuthController)
            .GetField("OnIdentityResolved", NonPubInst)
            ?.GetValue(controller) as System.Action<FactoryAuthController>;
        del?.Invoke(controller);

        // Wait for Load() coroutine to fail (network unreachable) and call Hide()
        float elapsed = 0f;
        while (root.style.display.value != DisplayStyle.None && elapsed < 5f)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        Assert.AreEqual(DisplayStyle.None, root.style.display.value,
            "Load() must have been called; network failure should cause CharacterIdleDisplay.Hide()");

        UnityEngine.Object.Destroy(widgetGo);
        UnityEngine.Object.Destroy(displayGo);
        UnityEngine.Object.Destroy(authGo);
    }

    [UnityTest]
    public IEnumerator OnDestroy_UnsubscribesFromAuthController()
    {
        var authGo = new GameObject();
        var controller = authGo.AddComponent<FactoryAuthController>();

        var displayGo = new GameObject();
        var charDisplay = displayGo.AddComponent<CharacterIdleDisplay>();

        var (widgetGo, widget) = CreateWidget(controller, charDisplay);

        var root = new VisualElement();
        widget.Initialize(root);

        yield return null;

        // Destroy the widget; subscription should be removed
        UnityEngine.Object.Destroy(widgetGo);
        yield return null;

        // Set CharacterName non-null and fire event; no exception should occur
        typeof(FactoryAuthController)
            .GetField("<CharacterName>k__BackingField", NonPubInst)
            ?.SetValue(controller, "testchar");

        var del = typeof(FactoryAuthController)
            .GetField("OnIdentityResolved", NonPubInst)
            ?.GetValue(controller) as System.Action<FactoryAuthController>;

        Assert.DoesNotThrow(() => del?.Invoke(controller),
            "Invoking the event after widget destroyed must not throw");

        UnityEngine.Object.Destroy(displayGo);
        UnityEngine.Object.Destroy(authGo);
    }
}
