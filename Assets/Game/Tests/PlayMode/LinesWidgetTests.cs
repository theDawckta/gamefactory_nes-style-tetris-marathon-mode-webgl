using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

public class LinesWidgetTests
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

    private void FireLinesChanged(PlayfieldController controller, int lines)
    {
        var field = typeof(PlayfieldController)
            .GetField("OnLinesChanged", BindingFlags.Instance | BindingFlags.NonPublic);
        var del = field?.GetValue(controller) as Action<int>;
        del?.Invoke(lines);
    }

    [UnityTest]
    public IEnumerator LinesWidget_AttachesToGameObject()
    {
        var go = Track(new GameObject());
        var widget = go.AddComponent<LinesWidget>();
        yield return null;
        Assert.IsNotNull(widget);
    }

    [UnityTest]
    public IEnumerator Initialize_PopulatesLinesRegion()
    {
        var go = Track(new GameObject());
        var widget = go.AddComponent<LinesWidget>();
        var controller = CreateController();
        yield return null;

        var region = new VisualElement();
        widget.Initialize(region, controller);

        Assert.AreEqual(1, region.childCount, "LinesRegion should have one container child");
    }

    [UnityTest]
    public IEnumerator Initialize_ContainsHeaderLabel()
    {
        var go = Track(new GameObject());
        var widget = go.AddComponent<LinesWidget>();
        var controller = CreateController();
        yield return null;

        var region = new VisualElement();
        widget.Initialize(region, controller);

        var container = region[0];
        var header = container[0] as Label;
        Assert.IsNotNull(header, "First child of container should be a Label");
        Assert.AreEqual("LINES", header.text, "Header label should read LINES");
    }

    [UnityTest]
    public IEnumerator Initialize_ContainsValueLabel()
    {
        var go = Track(new GameObject());
        var widget = go.AddComponent<LinesWidget>();
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
        var widget = go.AddComponent<LinesWidget>();
        yield return null;

        var region = new VisualElement();
        Assert.DoesNotThrow(() => widget.Initialize(region, null));
    }

    [UnityTest]
    public IEnumerator Initialize_WithNullRegion_DoesNotThrow()
    {
        var go = Track(new GameObject());
        var widget = go.AddComponent<LinesWidget>();
        var controller = CreateController();
        yield return null;

        Assert.DoesNotThrow(() => widget.Initialize(null, controller));
    }

    [UnityTest]
    public IEnumerator Initialize_OnLinesChanged_UpdatesValueLabel()
    {
        var go = Track(new GameObject());
        var widget = go.AddComponent<LinesWidget>();
        var controller = CreateController();
        yield return null;

        var region = new VisualElement();
        widget.Initialize(region, controller);

        var container = region[0];
        var valueLabel = container[1] as Label;
        Assert.IsNotNull(valueLabel);

        FireLinesChanged(controller, 10);

        Assert.AreEqual("10", valueLabel.text, "Value label should reflect new line count");
    }

    [UnityTest]
    public IEnumerator Initialize_OnLinesChanged_MultipleUpdates()
    {
        var go = Track(new GameObject());
        var widget = go.AddComponent<LinesWidget>();
        var controller = CreateController();
        yield return null;

        var region = new VisualElement();
        widget.Initialize(region, controller);

        var container = region[0];
        var valueLabel = container[1] as Label;

        FireLinesChanged(controller, 4);
        Assert.AreEqual("4", valueLabel.text);

        FireLinesChanged(controller, 8);
        Assert.AreEqual("8", valueLabel.text);

        FireLinesChanged(controller, 20);
        Assert.AreEqual("20", valueLabel.text);
    }

    [UnityTest]
    public IEnumerator OnDestroy_UnsubscribesFromEvent()
    {
        var go = Track(new GameObject());
        var widget = go.AddComponent<LinesWidget>();
        var controller = CreateController();
        yield return null;

        var region = new VisualElement();
        widget.Initialize(region, controller);

        UnityEngine.Object.Destroy(go);
        yield return null;

        // Firing the event after destroy should not throw
        Assert.DoesNotThrow(() => FireLinesChanged(controller, 9999));
    }
}
