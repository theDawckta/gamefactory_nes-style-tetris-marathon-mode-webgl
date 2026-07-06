using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

public class LevelWidgetTests
{
    private List<GameObject> _toDestroy;

    [SetUp]
    public void SetUp()
    {
        _toDestroy = new List<GameObject>();
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var go in _toDestroy)
            if (go != null) UnityEngine.Object.Destroy(go);
        _toDestroy.Clear();
    }

    private GameObject Track(GameObject go) { _toDestroy.Add(go); return go; }

    private PlayfieldController CreateController()
    {
        var go = Track(new GameObject());
        return go.AddComponent<PlayfieldController>();
    }

    private void FireLevelChanged(PlayfieldController controller, int level)
    {
        var field = typeof(PlayfieldController)
            .GetField("OnLevelChanged", BindingFlags.Instance | BindingFlags.NonPublic);
        var del = field?.GetValue(controller) as Action<int>;
        del?.Invoke(level);
    }

    [UnityTest]
    public IEnumerator LevelWidget_AttachesToGameObject()
    {
        var go = Track(new GameObject());
        var widget = go.AddComponent<LevelWidget>();
        yield return null;
        Assert.IsNotNull(widget);
    }

    [UnityTest]
    public IEnumerator Initialize_PopulatesLevelRegion()
    {
        var go = Track(new GameObject());
        var widget = go.AddComponent<LevelWidget>();
        var controller = CreateController();
        yield return null;

        var region = new VisualElement();
        widget.Initialize(region, controller);

        Assert.AreEqual(1, region.childCount, "LevelRegion should have one container child");
    }

    [UnityTest]
    public IEnumerator Initialize_ContainsHeaderLabel()
    {
        var go = Track(new GameObject());
        var widget = go.AddComponent<LevelWidget>();
        var controller = CreateController();
        yield return null;

        var region = new VisualElement();
        widget.Initialize(region, controller);

        var container = region[0];
        var header = container[0] as Label;
        Assert.IsNotNull(header, "First child of container should be a Label");
        Assert.AreEqual("LEVEL", header.text, "Header label should read LEVEL");
    }

    [UnityTest]
    public IEnumerator Initialize_ContainsValueLabel()
    {
        var go = Track(new GameObject());
        var widget = go.AddComponent<LevelWidget>();
        var controller = CreateController();
        yield return null;

        var region = new VisualElement();
        widget.Initialize(region, controller);

        var container = region[0];
        Assert.GreaterOrEqual(container.childCount, 2, "Container should have at least two labels");
        var valueLabel = container[1] as Label;
        Assert.IsNotNull(valueLabel, "Second child of container should be a Label");
        Assert.AreEqual("0", valueLabel.text, "Value label should start at 0");
    }

    [UnityTest]
    public IEnumerator Initialize_WithNullController_DoesNotThrow()
    {
        var go = Track(new GameObject());
        var widget = go.AddComponent<LevelWidget>();
        yield return null;

        var region = new VisualElement();
        Assert.DoesNotThrow(() => widget.Initialize(region, null));
    }

    [UnityTest]
    public IEnumerator Initialize_WithNullRegion_DoesNotThrow()
    {
        var go = Track(new GameObject());
        var widget = go.AddComponent<LevelWidget>();
        var controller = CreateController();
        yield return null;

        Assert.DoesNotThrow(() => widget.Initialize(null, controller));
    }

    [UnityTest]
    public IEnumerator Initialize_OnLevelChanged_UpdatesValueLabel()
    {
        var go = Track(new GameObject());
        var widget = go.AddComponent<LevelWidget>();
        var controller = CreateController();
        yield return null;

        var region = new VisualElement();
        widget.Initialize(region, controller);

        var container = region[0];
        var valueLabel = container[1] as Label;
        Assert.IsNotNull(valueLabel);

        FireLevelChanged(controller, 5);

        Assert.AreEqual("5", valueLabel.text, "Value label should reflect new level");
    }

    [UnityTest]
    public IEnumerator Initialize_OnLevelChanged_MultipleUpdates()
    {
        var go = Track(new GameObject());
        var widget = go.AddComponent<LevelWidget>();
        var controller = CreateController();
        yield return null;

        var region = new VisualElement();
        widget.Initialize(region, controller);

        var container = region[0];
        var valueLabel = container[1] as Label;

        FireLevelChanged(controller, 1);
        Assert.AreEqual("1", valueLabel.text);

        FireLevelChanged(controller, 5);
        Assert.AreEqual("5", valueLabel.text);

        FireLevelChanged(controller, 9);
        Assert.AreEqual("9", valueLabel.text);
    }

    [UnityTest]
    public IEnumerator OnDestroy_UnsubscribesFromEvent()
    {
        var go = Track(new GameObject());
        var widget = go.AddComponent<LevelWidget>();
        var controller = CreateController();
        yield return null;

        var region = new VisualElement();
        widget.Initialize(region, controller);

        UnityEngine.Object.Destroy(go);
        yield return null;

        // Firing the event after destroy should not throw
        Assert.DoesNotThrow(() => FireLevelChanged(controller, 99));
    }
}
