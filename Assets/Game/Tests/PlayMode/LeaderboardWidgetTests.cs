using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

public class LeaderboardWidgetTests
{
    private List<GameObject> _toDestroy;

    [SetUp]
    public void SetUp()
    {
        _toDestroy = new List<GameObject>();
        if (LeaderboardClient.Instance != null)
            Object.Destroy(LeaderboardClient.Instance.gameObject);
        if (ConfigService.Instance != null)
            Object.Destroy(ConfigService.Instance.gameObject);
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var go in _toDestroy)
            if (go != null) Object.Destroy(go);
        _toDestroy.Clear();
        if (LeaderboardClient.Instance != null)
            Object.Destroy(LeaderboardClient.Instance.gameObject);
        if (ConfigService.Instance != null)
            Object.Destroy(ConfigService.Instance.gameObject);
    }

    private GameObject Track(GameObject go) { _toDestroy.Add(go); return go; }

    private static VisualElement CreateTestRegion()
    {
        var region = new VisualElement();
        for (int i = 0; i < 5; i++)
        {
            var row = new VisualElement();
            var rank = new Label(); rank.name = "rank";
            var score = new Label(); score.name = "score";
            var username = new Label(); username.name = "username";
            var charSlot = new VisualElement(); charSlot.name = "characterSlot";
            row.Add(rank);
            row.Add(score);
            row.Add(username);
            row.Add(charSlot);
            region.Add(row);
        }
        return region;
    }

    [UnityTest]
    public IEnumerator LeaderboardWidget_AttachesToGameObject()
    {
        var go = Track(new GameObject());
        var widget = go.AddComponent<LeaderboardWidget>();
        yield return null;
        Assert.IsNotNull(widget);
    }

    [UnityTest]
    public IEnumerator Awake_CreatesCharacterIdleDisplayChildren()
    {
        var go = Track(new GameObject());
        go.AddComponent<LeaderboardWidget>();
        yield return null;
        var displays = go.GetComponentsInChildren<CharacterIdleDisplay>(true);
        Assert.AreEqual(5, displays.Length);
    }

    [UnityTest]
    public IEnumerator SetLeaderboardRegion_Null_DoesNotThrow()
    {
        var go = Track(new GameObject());
        var widget = go.AddComponent<LeaderboardWidget>();
        yield return null;
        Assert.DoesNotThrow(() => widget.SetLeaderboardRegion(null));
    }

    [UnityTest]
    public IEnumerator SetLeaderboardRegion_WithValidRegion_WiresDisplayRoots()
    {
        var go = Track(new GameObject());
        var widget = go.AddComponent<LeaderboardWidget>();
        yield return null;

        widget.SetLeaderboardRegion(CreateTestRegion());

        var displays = go.GetComponentsInChildren<CharacterIdleDisplay>(true);
        for (int i = 0; i < 5; i++)
            Assert.IsNotNull(displays[i].Root, $"CharacterIdleDisplay[{i}].Root should be wired to row {i}'s characterSlot");
    }

    [UnityTest]
    public IEnumerator SetLeaderboardRegion_WithValidRegion_HidesAllCharSlots()
    {
        var go = Track(new GameObject());
        var widget = go.AddComponent<LeaderboardWidget>();
        yield return null;

        widget.SetLeaderboardRegion(CreateTestRegion());

        var displays = go.GetComponentsInChildren<CharacterIdleDisplay>(true);
        for (int i = 0; i < 5; i++)
            Assert.AreEqual(DisplayStyle.None, displays[i].Root.style.display.value,
                $"CharacterIdleDisplay[{i}] should be hidden after SetLeaderboardRegion");
    }

    [UnityTest]
    public IEnumerator Refresh_WithNullClient_DoesNotThrow()
    {
        var go = Track(new GameObject());
        var widget = go.AddComponent<LeaderboardWidget>();
        yield return null;
        widget.SetLeaderboardRegion(CreateTestRegion());
        Assert.DoesNotThrow(() => widget.Refresh());
    }

    [UnityTest]
    public IEnumerator Refresh_WithNullRegion_DoesNotThrow()
    {
        var go = Track(new GameObject());
        var widget = go.AddComponent<LeaderboardWidget>();
        yield return null;
        Assert.DoesNotThrow(() => widget.Refresh());
    }

    [UnityTest]
    public IEnumerator Refresh_WhenFetchFails_ClearsAllRowLabels()
    {
        // LeaderboardClient with no leaderboardUrl configured -> FetchTopFive calls onError
        var clientGo = Track(new GameObject());
        var client = clientGo.AddComponent<LeaderboardClient>();
        yield return null; // Awake sets singleton

        var widgetGo = Track(new GameObject());
        var widget = widgetGo.AddComponent<LeaderboardWidget>();

        // Wire client via reflection (serialized field not settable from code otherwise)
        typeof(LeaderboardWidget)
            .GetField("_leaderboardClient", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(widget, client);

        yield return null; // Start() runs

        // Build region with pre-filled labels to verify clearing
        var region = CreateTestRegion();
        ((Label)region[0][0]).text = "1.";
        ((Label)region[0][1]).text = "99999";
        ((Label)region[0][2]).text = "Player1";
        ((Label)region[1][0]).text = "2.";

        widget.SetLeaderboardRegion(region);
        widget.Refresh();

        // Wait for FetchTopFive to call onError and ClearAllRows to run (up to 5s)
        float elapsed = 0f;
        while (((Label)region[0][0]).text != "" && elapsed < 5f)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        Assert.AreEqual("", ((Label)region[0][0]).text, "Row 0 rank should be cleared on fetch error");
        Assert.AreEqual("", ((Label)region[0][1]).text, "Row 0 score should be cleared on fetch error");
        Assert.AreEqual("", ((Label)region[0][2]).text, "Row 0 username should be cleared on fetch error");
        Assert.AreEqual("", ((Label)region[1][0]).text, "Row 1 rank should be cleared on fetch error");
    }
}
