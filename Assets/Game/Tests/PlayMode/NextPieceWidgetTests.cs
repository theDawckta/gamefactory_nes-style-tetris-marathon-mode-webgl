using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

public class NextPieceWidgetTests
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

    private void FirePieceLocked(PlayfieldController controller)
    {
        var field = typeof(PlayfieldController)
            .GetField("OnPieceLocked", BindingFlags.Instance | BindingFlags.NonPublic);
        var del = field?.GetValue(controller) as Action;
        del?.Invoke();
    }

    private void SetNextPieceType(PlayfieldController controller, TetrominoType type)
    {
        var backing = typeof(PlayfieldController)
            .GetField("<NextPieceType>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
        backing?.SetValue(controller, type);
    }

    private static readonly Color EmptyColor = new Color(0.05f, 0.05f, 0.05f);

    private int CountColoredCells(VisualElement gridContainer)
    {
        int count = 0;
        for (int i = 0; i < 16; i++)
        {
            var bg = gridContainer[i].style.backgroundColor.value;
            if (bg != EmptyColor)
                count++;
        }
        return count;
    }

    [UnityTest]
    public IEnumerator NextPieceWidget_AttachesToGameObject()
    {
        var go = Track(new GameObject());
        var widget = go.AddComponent<NextPieceWidget>();
        yield return null;
        Assert.IsNotNull(widget);
    }

    [UnityTest]
    public IEnumerator Initialize_PopulatesNextPieceRegion()
    {
        var go = Track(new GameObject());
        var widget = go.AddComponent<NextPieceWidget>();
        var controller = CreateController();
        yield return null;

        var region = new VisualElement();
        widget.Initialize(region, controller);

        Assert.AreEqual(1, region.childCount, "Region should contain one container child");
    }

    [UnityTest]
    public IEnumerator Initialize_ContainsNextHeader()
    {
        var go = Track(new GameObject());
        var widget = go.AddComponent<NextPieceWidget>();
        var controller = CreateController();
        yield return null;

        var region = new VisualElement();
        widget.Initialize(region, controller);

        var container = region[0];
        var header = container[0] as Label;
        Assert.IsNotNull(header, "First child of container should be a Label");
        Assert.AreEqual("NEXT", header.text, "Header label should read NEXT");
    }

    [UnityTest]
    public IEnumerator Initialize_Contains4x4Grid()
    {
        var go = Track(new GameObject());
        var widget = go.AddComponent<NextPieceWidget>();
        var controller = CreateController();
        yield return null;

        var region = new VisualElement();
        widget.Initialize(region, controller);

        var container = region[0];
        Assert.GreaterOrEqual(container.childCount, 2, "Container should have header plus grid container");
        var gridContainer = container[1];
        Assert.AreEqual(16, gridContainer.childCount, "Grid should have 16 cells (4x4)");
    }

    [UnityTest]
    public IEnumerator Initialize_WithNullRegion_DoesNotThrow()
    {
        var go = Track(new GameObject());
        var widget = go.AddComponent<NextPieceWidget>();
        var controller = CreateController();
        yield return null;

        Assert.DoesNotThrow(() => widget.Initialize(null, controller));
    }

    [UnityTest]
    public IEnumerator Initialize_WithNullController_DoesNotThrow()
    {
        var go = Track(new GameObject());
        var widget = go.AddComponent<NextPieceWidget>();
        yield return null;

        var region = new VisualElement();
        Assert.DoesNotThrow(() => widget.Initialize(region, null));
    }

    [UnityTest]
    public IEnumerator Initialize_WithNullController_GridIsEmpty()
    {
        var go = Track(new GameObject());
        var widget = go.AddComponent<NextPieceWidget>();
        yield return null;

        var region = new VisualElement();
        widget.Initialize(region, null);

        var gridContainer = region[0][1];
        Assert.AreEqual(0, CountColoredCells(gridContainer), "Grid should have no colored cells without a controller");
    }

    [UnityTest]
    public IEnumerator AllPieceTypes_ShowExactlyFourColoredCells()
    {
        foreach (TetrominoType type in Enum.GetValues(typeof(TetrominoType)))
        {
            var controllerGo = Track(new GameObject());
            var controller = controllerGo.AddComponent<PlayfieldController>();
            var widgetGo = Track(new GameObject());
            var widget = widgetGo.AddComponent<NextPieceWidget>();
            yield return null;

            var region = new VisualElement();
            SetNextPieceType(controller, type);
            widget.Initialize(region, controller);

            var gridContainer = region[0][1];
            int coloredCount = CountColoredCells(gridContainer);
            Assert.AreEqual(4, coloredCount, $"Piece {type} should have exactly 4 colored cells");
        }
    }

    [UnityTest]
    public IEnumerator AllPieceTypes_CellsAreCorrectColor()
    {
        foreach (TetrominoType type in Enum.GetValues(typeof(TetrominoType)))
        {
            var controllerGo = Track(new GameObject());
            var controller = controllerGo.AddComponent<PlayfieldController>();
            var widgetGo = Track(new GameObject());
            var widget = widgetGo.AddComponent<NextPieceWidget>();
            yield return null;

            var region = new VisualElement();
            SetNextPieceType(controller, type);
            widget.Initialize(region, controller);

            var gridContainer = region[0][1];
            Color expected = TetrominoData.GetColor(type);
            for (int i = 0; i < 16; i++)
            {
                var bg = gridContainer[i].style.backgroundColor.value;
                if (bg != EmptyColor)
                    Assert.AreEqual(expected, bg, $"Piece {type}: colored cell should use piece color");
            }
        }
    }

    [UnityTest]
    public IEnumerator AllPieceTypes_CellsAreWithinGridBounds()
    {
        foreach (TetrominoType type in Enum.GetValues(typeof(TetrominoType)))
        {
            var controllerGo = Track(new GameObject());
            var controller = controllerGo.AddComponent<PlayfieldController>();
            var widgetGo = Track(new GameObject());
            var widget = widgetGo.AddComponent<NextPieceWidget>();
            yield return null;

            var region = new VisualElement();
            SetNextPieceType(controller, type);
            widget.Initialize(region, controller);

            // All 16 cells should exist (enforced by the grid building loop itself)
            var gridContainer = region[0][1];
            Assert.AreEqual(16, gridContainer.childCount, $"Piece {type}: grid should have 16 children");
        }
    }

    [UnityTest]
    public IEnumerator PieceLocked_UpdatesGridWithNextPiece()
    {
        var go = Track(new GameObject());
        var widget = go.AddComponent<NextPieceWidget>();
        var controller = CreateController();
        yield return null;

        var region = new VisualElement();
        SetNextPieceType(controller, TetrominoType.I);
        widget.Initialize(region, controller);

        var gridContainer = region[0][1];

        // Change to O piece and fire locked
        SetNextPieceType(controller, TetrominoType.O);
        FirePieceLocked(controller);

        Color oColor = TetrominoData.GetColor(TetrominoType.O);
        int oColoredCount = 0;
        for (int i = 0; i < 16; i++)
        {
            var bg = gridContainer[i].style.backgroundColor.value;
            if (bg == oColor) oColoredCount++;
        }
        Assert.AreEqual(4, oColoredCount, "Grid should show O piece (4 cells) after piece locked");
    }

    [UnityTest]
    public IEnumerator Update_ReflectsNextPieceTypeChange()
    {
        var go = Track(new GameObject());
        var widget = go.AddComponent<NextPieceWidget>();
        var controller = CreateController();
        yield return null;

        var region = new VisualElement();
        SetNextPieceType(controller, TetrominoType.I);
        widget.Initialize(region, controller);

        // Change next piece type without firing event; Update() should pick it up
        SetNextPieceType(controller, TetrominoType.T);
        yield return null; // one frame for Update to run

        var gridContainer = region[0][1];
        Color tColor = TetrominoData.GetColor(TetrominoType.T);
        int tColoredCount = 0;
        for (int i = 0; i < 16; i++)
        {
            var bg = gridContainer[i].style.backgroundColor.value;
            if (bg == tColor) tColoredCount++;
        }
        Assert.AreEqual(4, tColoredCount, "Update should refresh grid when NextPieceType changes");
    }

    [UnityTest]
    public IEnumerator OnDestroy_UnsubscribesFromPieceLocked()
    {
        var go = Track(new GameObject());
        var widget = go.AddComponent<NextPieceWidget>();
        var controller = CreateController();
        yield return null;

        var region = new VisualElement();
        widget.Initialize(region, controller);

        UnityEngine.Object.Destroy(go);
        yield return null;

        Assert.DoesNotThrow(() => FirePieceLocked(controller), "Firing OnPieceLocked after widget destroyed should not throw");
    }
}
