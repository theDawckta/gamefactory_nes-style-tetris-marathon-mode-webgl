using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

public class PlayfieldRendererTests
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
            if (go != null) Object.Destroy(go);
        _toDestroy.Clear();
    }

    private GameObject Track(GameObject go) { _toDestroy.Add(go); return go; }

    private VisualElement[,] GetCellElements(PlayfieldRenderer renderer)
    {
        return (VisualElement[,])typeof(PlayfieldRenderer)
            .GetField("_cellElements", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(renderer);
    }

    private TetrominoType?[,] GetGrid(PlayfieldController controller)
    {
        return (TetrominoType?[,])typeof(PlayfieldController)
            .GetField("_grid", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(controller);
    }

    // ── Initialize ────────────────────────────────────────────────────────────

    [UnityTest]
    public IEnumerator Initialize_Creates200ChildrenInRegion()
    {
        var go = Track(new GameObject());
        var renderer = go.AddComponent<PlayfieldRenderer>();
        var controller = go.AddComponent<PlayfieldController>();
        var region = new VisualElement();
        yield return null;
        renderer.Initialize(region, controller);
        Assert.AreEqual(200, region.childCount);
    }

    [UnityTest]
    public IEnumerator Initialize_AllCellsAreAbsolutePositioned()
    {
        var go = Track(new GameObject());
        var renderer = go.AddComponent<PlayfieldRenderer>();
        var controller = go.AddComponent<PlayfieldController>();
        var region = new VisualElement();
        yield return null;
        renderer.Initialize(region, controller);
        for (int i = 0; i < region.childCount; i++)
            Assert.AreEqual(Position.Absolute, region[i].style.position.value,
                $"Cell {i} must be absolute positioned");
    }

    [UnityTest]
    public IEnumerator Initialize_AllCellsAre20x20Pixels()
    {
        var go = Track(new GameObject());
        var renderer = go.AddComponent<PlayfieldRenderer>();
        var controller = go.AddComponent<PlayfieldController>();
        var region = new VisualElement();
        yield return null;
        renderer.Initialize(region, controller);
        for (int i = 0; i < region.childCount; i++)
        {
            Assert.AreEqual(20f, region[i].style.width.value.value, $"Cell {i} width");
            Assert.AreEqual(20f, region[i].style.height.value.value, $"Cell {i} height");
        }
    }

    [UnityTest]
    public IEnumerator Initialize_SecondCall_StillProduces200Children()
    {
        var go = Track(new GameObject());
        var renderer = go.AddComponent<PlayfieldRenderer>();
        var controller = go.AddComponent<PlayfieldController>();
        var region = new VisualElement();
        yield return null;
        renderer.Initialize(region, controller);
        renderer.Initialize(region, controller);
        Assert.AreEqual(200, region.childCount);
    }

    [UnityTest]
    public IEnumerator Initialize_RowZeroAtBottom_CellY0HasTopAt380()
    {
        var go = Track(new GameObject());
        var renderer = go.AddComponent<PlayfieldRenderer>();
        var controller = go.AddComponent<PlayfieldController>();
        var region = new VisualElement();
        yield return null;
        renderer.Initialize(region, controller);
        var cells = GetCellElements(renderer);
        Assert.AreEqual(380f, cells[0, 0].style.top.value.value);
    }

    [UnityTest]
    public IEnumerator Initialize_RowNineteenAtTop_CellY19HasTopAtZero()
    {
        var go = Track(new GameObject());
        var renderer = go.AddComponent<PlayfieldRenderer>();
        var controller = go.AddComponent<PlayfieldController>();
        var region = new VisualElement();
        yield return null;
        renderer.Initialize(region, controller);
        var cells = GetCellElements(renderer);
        Assert.AreEqual(0f, cells[0, 19].style.top.value.value);
    }

    [UnityTest]
    public IEnumerator Initialize_EmptyCells_HaveDarkBackground()
    {
        var go = Track(new GameObject());
        var renderer = go.AddComponent<PlayfieldRenderer>();
        var controller = go.AddComponent<PlayfieldController>();
        var region = new VisualElement();
        yield return null;
        renderer.Initialize(region, controller);
        var cells = GetCellElements(renderer);
        var bg = cells[0, 0].style.backgroundColor.value;
        Assert.Less(bg.r, 0.3f, "Empty cell red channel should be dark");
        Assert.Less(bg.g, 0.3f, "Empty cell green channel should be dark");
        Assert.Less(bg.b, 0.3f, "Empty cell blue channel should be dark");
    }

    // ── Rendering: locked cells ───────────────────────────────────────────────

    [UnityTest]
    public IEnumerator Update_LockedCell_DisplaysTetrominoColor()
    {
        var go = Track(new GameObject());
        var renderer = go.AddComponent<PlayfieldRenderer>();
        var controller = go.AddComponent<PlayfieldController>();
        var region = new VisualElement();
        controller.StartGame();
        yield return null;
        renderer.Initialize(region, controller);
        renderer.SetActive(true);

        var grid = GetGrid(controller);
        grid[0, 0] = TetrominoType.I;

        yield return null; // Update renders

        var cells = GetCellElements(renderer);
        var bg = cells[0, 0].style.backgroundColor.value;
        var expected = TetrominoData.GetColor(TetrominoType.I);
        Assert.AreEqual(expected.r, bg.r, 0.01f, "Red channel");
        Assert.AreEqual(expected.g, bg.g, 0.01f, "Green channel");
        Assert.AreEqual(expected.b, bg.b, 0.01f, "Blue channel");
    }

    [UnityTest]
    public IEnumerator Update_EmptyCell_HasDarkBackground()
    {
        var go = Track(new GameObject());
        var renderer = go.AddComponent<PlayfieldRenderer>();
        var controller = go.AddComponent<PlayfieldController>();
        var region = new VisualElement();
        controller.StartGame();
        yield return null;
        renderer.Initialize(region, controller);
        renderer.SetActive(true);
        yield return null;

        var cells = GetCellElements(renderer);
        // Cell (0, 0) is never part of the spawn area so it stays empty
        var bg = cells[0, 0].style.backgroundColor.value;
        Assert.Less(bg.r, 0.3f, "Empty cell should have dark background");
        Assert.Less(bg.g, 0.3f, "Empty cell should have dark background");
        Assert.Less(bg.b, 0.3f, "Empty cell should have dark background");
    }

    // ── Rendering: active piece ───────────────────────────────────────────────

    [UnityTest]
    public IEnumerator Update_ActivePiece_DisplaysBrighterTintAtCurrentPosition()
    {
        var go = Track(new GameObject());
        var renderer = go.AddComponent<PlayfieldRenderer>();
        var controller = go.AddComponent<PlayfieldController>();
        var region = new VisualElement();
        controller.StartGame();
        yield return null;
        renderer.Initialize(region, controller);
        renderer.SetActive(true);
        yield return null; // Update renders

        var cells = GetCellElements(renderer);
        var pos = controller.CurrentPiecePosition;
        var pieces = TetrominoData.GetCells(controller.CurrentPieceType, controller.CurrentPieceRotation);
        Color rawColor = TetrominoData.GetColor(controller.CurrentPieceType);
        Color expectedColor = Color.Lerp(rawColor, Color.white, 0.25f);

        foreach (var c in pieces)
        {
            int cx = pos.x + c.x;
            int cy = pos.y + c.y;
            if (cx < 0 || cx >= PlayfieldController.GridWidth || cy < 0 || cy >= PlayfieldController.GridHeight) continue;

            var bg = cells[cx, cy].style.backgroundColor.value;
            Assert.AreEqual(expectedColor.r, bg.r, 0.01f, $"Active piece cell ({cx},{cy}) red");
            Assert.AreEqual(expectedColor.g, bg.g, 0.01f, $"Active piece cell ({cx},{cy}) green");
            Assert.AreEqual(expectedColor.b, bg.b, 0.01f, $"Active piece cell ({cx},{cy}) blue");
        }
    }

    // ── SetActive ─────────────────────────────────────────────────────────────

    [UnityTest]
    public IEnumerator SetActive_False_StopsRendering()
    {
        var go = Track(new GameObject());
        var renderer = go.AddComponent<PlayfieldRenderer>();
        var controller = go.AddComponent<PlayfieldController>();
        var region = new VisualElement();
        controller.StartGame();
        yield return null;
        renderer.Initialize(region, controller);
        renderer.SetActive(false);

        var cells = GetCellElements(renderer);
        var initialBg = cells[0, 0].style.backgroundColor.value;

        // Place a locked piece -- renderer should not pick it up
        var grid = GetGrid(controller);
        grid[0, 0] = TetrominoType.I;

        yield return null; // Update should be suppressed

        var bgAfter = cells[0, 0].style.backgroundColor.value;
        Assert.AreEqual(initialBg.r, bgAfter.r, 0.01f, "Cell should not update when inactive");
        Assert.AreEqual(initialBg.g, bgAfter.g, 0.01f, "Cell should not update when inactive");
        Assert.AreEqual(initialBg.b, bgAfter.b, 0.01f, "Cell should not update when inactive");
    }

    [UnityTest]
    public IEnumerator SetActive_True_ResumesRendering()
    {
        var go = Track(new GameObject());
        var renderer = go.AddComponent<PlayfieldRenderer>();
        var controller = go.AddComponent<PlayfieldController>();
        var region = new VisualElement();
        controller.StartGame();
        yield return null;
        renderer.Initialize(region, controller);
        renderer.SetActive(false);
        yield return null;

        // Place locked piece while inactive
        var grid = GetGrid(controller);
        grid[0, 0] = TetrominoType.I;
        yield return null; // still not rendered

        renderer.SetActive(true);
        yield return null; // now Update renders

        var cells = GetCellElements(renderer);
        var bg = cells[0, 0].style.backgroundColor.value;
        var expected = TetrominoData.GetColor(TetrominoType.I);
        Assert.AreEqual(expected.r, bg.r, 0.01f);
        Assert.AreEqual(expected.g, bg.g, 0.01f);
        Assert.AreEqual(expected.b, bg.b, 0.01f);
    }
}
